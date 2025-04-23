import pandas as pd
import numpy as np
from sqlalchemy import create_engine, text
from scipy.cluster.hierarchy import dendrogram, linkage, fcluster
import matplotlib.pyplot as plt
from collections import defaultdict
import logging
from tqdm import tqdm
from sklearn_extra.cluster import KMedoids
from datetime import datetime
import argparse
import os
import time
from tenacity import retry, stop_after_attempt, wait_exponential
import psutil
from concurrent.futures import ProcessPoolExecutor
from math import ceil
import gc
import json
import shutil
import sys
import traceback
import base64
import io
from contextlib import redirect_stderr
import contextlib
import tempfile


# logs 디렉토리가 없으면 생성
log_dir = 'python_log'
if not os.path.exists(log_dir):
    os.makedirs(log_dir)

# 현재 날짜로 파일명 생성
current_date = datetime.now().strftime('%Y-%m-%d')
log_filename = f'{current_date}_clustering.log'

logging.basicConfig(
    filename=os.path.join(log_dir, log_filename),
    level=logging.INFO,
    format='%(asctime)s - %(levelname)s - %(message)s'
)

class GroupClusterAnalyzer:
    def __init__(self, db_config, lv1, lv2, threshold=None , sub_threshold=None):
        self.db_config = db_config
        self.data_groups = None
        self.similarity_matrix = None
        self.clusters_lv3 = None
        self.clusters_lv4 = None
        self.lv1 = lv1
        self.lv2 = lv2
        self.threshold = threshold
        self.sub_threshold = sub_threshold
        self.use_lv4 = False
        logging.info(f"Initialized with lv1={lv1}, lv2={lv2}, threshold={threshold}")

    def prepare_data_from_json(self, json_data):
        """JSON 데이터로부터 DataFrame 생성"""
        try:
            # JSON 데이터를 DataFrame으로 변환
            df = pd.DataFrame(json_data)
            
            # 컬럼 이름 변경
            df = df.rename(columns={
                'DataId': 'data_id',
                'KeywordString': 'original_text'
            })
            
            # keyword_list 컬럼 생성
            df['keyword_list'] = df['original_text'].apply(lambda x: x.split(','))
            
            self.data_groups = df
            logging.info(f"Prepared {len(df)} records from JSON data")
            
        except Exception as e:
            logging.error(f"Error preparing data from JSON: {str(e)}")
            raise
    
    def calculate_group_similarity_matrix(self):
        """태스크별 메모리 맵 관리가 포함된 유사도 매트릭스 계산"""
        try:
            
            n = len(self.data_groups)
            chunk_size = 1000
            
            # 태스크별 디렉토리 설정
            task_dir = f'task_lv1_{self.lv1}_lv2_{self.lv2}'
            if os.path.exists(task_dir):
                # 기존 태스크 디렉토리가 있으면 삭제
                shutil.rmtree(task_dir)
            os.makedirs(task_dir)
            
            # 태스크별 메모리 맵 파일 경로
            mmap_filename = os.path.join(task_dir, f'similarity_matrix_lv1_{self.lv1}_lv2_{self.lv2}.mmap')
            
            # 새로운 메모리 맵 생성
            self.similarity_matrix = np.memmap(mmap_filename, dtype=np.float32, 
                                            mode='w+', shape=(n, n))
            self.similarity_matrix.fill(0)
            logging.info(f"Created new memory map with shape {self.similarity_matrix.shape} "
                        f"for task lv1={self.lv1}, lv2={self.lv2}")
            
            # 태스크별 임시 디렉토리 설정
            temp_dir = os.path.join(task_dir, 'temp_chunks')
            os.makedirs(temp_dir, exist_ok=True)
            
            # 진행 상황 파일
            progress_file = os.path.join(temp_dir, 'progress.txt')
            completed_chunks = set()
            
            # 키워드 셋 준비
            keyword_sets = [set(keywords) for keywords in self.data_groups['keyword_list']]
            
            # 실제 데이터 크기 로깅
            logging.info(f"Processing similarity matrix for {n} items")
            logging.info(f"Matrix shape: {self.similarity_matrix.shape}")
            
            # 청크 단위로 처리
            total_chunks = (n + chunk_size - 1) // chunk_size
            
            for chunk_idx in tqdm(range(total_chunks), desc="Processing chunks"):
                if chunk_idx in completed_chunks:
                    continue
                    
                start_i = chunk_idx * chunk_size
                end_i = min((chunk_idx + 1) * chunk_size, n)
                current_chunk_size = end_i - start_i
                
                # 현재 청크의 키워드 셋
                chunk_keywords = keyword_sets[start_i:end_i]
                
                # 더 작은 내부 청크로 처리
                inner_chunk_size = 100
                for j in range(0, n, inner_chunk_size):
                    j_end = min(j + inner_chunk_size, n)
                    current_keywords = keyword_sets[j:j_end]
                    
                    for k, kw1 in enumerate(chunk_keywords):
                        for l, kw2 in enumerate(current_keywords):
                            intersection = len(kw1 & kw2)
                            union = len(kw1 | kw2)
                            similarity = intersection / union if union > 0 else 0
                            
                            # 메모리 맵에 직접 쓰기
                            self.similarity_matrix[start_i + k, j + l] = similarity
                            self.similarity_matrix[j + l, start_i + k] = similarity  # 대칭성 유지
                    
                    # 메모리 맵 동기화
                    self.similarity_matrix.flush()
                    
                    # 중간 메모리 정리
                    if j % 1000 == 0:
                        gc.collect()
                
                # 진행 상황 저장
                with open(progress_file, 'a') as f:
                    f.write(f"{chunk_idx}\n")
                
                # 메모리 사용량 모니터링
                process = psutil.Process()
                memory_usage = process.memory_info().rss / (1024 * 1024)  # MB
                logging.info(f"Completed chunk {chunk_idx + 1}/{total_chunks}, "
                        f"Memory usage: {memory_usage:.2f} MB")
                
                # 청크 처리 후 메모리 정리
                gc.collect()
            
            logging.info("Similarity matrix calculation completed")
            
        except Exception as e:
            logging.error(f"Error in similarity matrix calculation: {str(e)}")
            raise
        finally:
            # 메모리 맵 동기화
            if hasattr(self, 'similarity_matrix'):
                self.similarity_matrix.flush()


    def perform_clustering(self, n_clusters_lv3=20, n_clusters_lv4=None):
        """메모리 맵을 활용한 최적화된 2단계 군집화 수행"""
        try:
            from scipy.spatial.distance import squareform
            import os
            import numpy as np
            
            # 데이터 유효성 검사
            if len(self.data_groups) == 0:
                logging.warning(f"No data found for lv1={self.lv1}, lv2={self.lv2}")
                return None
                
            if len(self.data_groups) < n_clusters_lv3:
                logging.warning(f"Not enough data for clustering. Found {len(self.data_groups)} items, but {n_clusters_lv3} clusters requested")
                return None
            
            # 태스크별 디렉토리 경로
            task_dir = f'task_lv1_{self.lv1}_lv2_{self.lv2}'
            
            cluster_stats = {
                'lv3': defaultdict(dict),
                'lv4': defaultdict(dict) if n_clusters_lv4 else None
            }
            
            # LV3 클러스터링
            logging.info("Starting LV3 clustering...")
            
            # 거리 행렬을 메모리 맵으로 생성
            distance_mmap_file = os.path.join(task_dir, 'distance_matrix.mmap')
            n = len(self.data_groups)
            
            # 청크 단위로 거리 행렬 계산
            distance_matrix = np.memmap(distance_mmap_file, dtype=np.float32, 
                                    mode='w+', shape=(n, n))
            
            chunk_size = 1000
            for i in range(0, n, chunk_size):
                end_i = min(i + chunk_size, n)
                distance_matrix[i:end_i] = 1 - self.similarity_matrix[i:end_i]
                distance_matrix.flush()
            
            logging.info("Distance matrix calculation completed")
            
            # 거리 행렬을 압축된 형식으로 변환
            logging.info("Converting to condensed format...")
            condensed_distance = squareform(distance_matrix)
            
            # 메모리 맵 파일 정리
            del distance_matrix
            if os.path.exists(distance_mmap_file):
                os.remove(distance_mmap_file)
            
            # 계층적 클러스터링 수행
            logging.info("Performing hierarchical clustering...")
            linkage_matrix = linkage(condensed_distance, method='ward')
            
            if self.threshold is not None:
                self.clusters_lv3 = fcluster(linkage_matrix, self.threshold, criterion='distance')
                n_clusters_actual = len(np.unique(self.clusters_lv3))
                logging.info(f"LV3 clustering with threshold {self.threshold} resulted in {n_clusters_actual} clusters")
            else:
                self.clusters_lv3 = fcluster(linkage_matrix, n_clusters_lv3, criterion='maxclust')
                
            self.data_groups['cluster_lv3'] = self.clusters_lv3
            logging.info("LV3 clustering completed")
            
            # LV4 클러스터링 (선택적)
            if n_clusters_lv4 is not None and n_clusters_lv4 > 0:
                logging.info("Starting LV4 clustering...")
                self.use_lv4 = True
                self.clusters_lv4 = np.zeros_like(self.clusters_lv3)
                
                # 각 LV3 클러스터에 대해 LV4 클러스터링 수행
                for lv3_cluster in range(1, max(self.clusters_lv3) + 1):
                    cluster_mask = self.data_groups['cluster_lv3'] == lv3_cluster
                    cluster_size = np.sum(cluster_mask)
                    
                    if cluster_size > n_clusters_lv4:
                        subset_indices = np.where(cluster_mask)[0]
                        subset_matrix = self.similarity_matrix[np.ix_(subset_indices, subset_indices)]
                        
                        # 부분 거리 행렬 계산
                        subset_distance = 1 - subset_matrix
                        subset_condensed = squareform(subset_distance)
                        
                        subset_linkage = linkage(subset_condensed, method='ward')
                        #subset_clusters = fcluster(subset_linkage, n_clusters_lv4, criterion='maxclust')
                        if self.sub_threshold is not None:
                            subset_clusters = fcluster(subset_linkage, self.sub_threshold, criterion='distance')
                            sub_n_clusters_actual = len(np.unique(subset_clusters))
                            logging.info(f"LV4 clustering with threshold {self.sub_threshold} resulted in {sub_n_clusters_actual} clusters")
                        else:
                            subset_clusters = fcluster(subset_linkage, n_clusters_lv4, criterion='maxclust')
                        
                        self.clusters_lv4[subset_indices] = lv3_cluster * 100 + subset_clusters
                    else:
                        self.clusters_lv4[cluster_mask] = lv3_cluster * 100
                
                self.data_groups['cluster_lv4'] = self.clusters_lv4
                logging.info("LV4 clustering completed")
            else:
                logging.info("Skipping LV4 clustering")
            
            # 클러스터 통계 생성
            logging.info("Generating cluster statistics...")
            
            # LV3 클러스터 통계
            for cluster_id in range(1, max(self.clusters_lv3) + 1):
                cluster_data = self.data_groups[self.data_groups['cluster_lv3'] == cluster_id]
                
                if len(cluster_data) > 0:
                    all_keywords = [kw for keywords in cluster_data['keyword_list'] for kw in keywords]
                    keyword_freq = pd.Series(all_keywords).value_counts()
                    
                    cluster_name = self.extract_unique_keywords(keyword_freq.to_dict())
                    self.data_groups.loc[self.data_groups['cluster_lv3'] == cluster_id, 'cluster_lv3_name'] = cluster_name
                    
                    cluster_stats['lv3'][cluster_id] = {
                        'size': len(cluster_data),
                        'name': cluster_name,
                        'common_keywords': keyword_freq.head(5).to_dict(),
                        'sample_texts': cluster_data['original_text'].head(3).tolist()
                    }
            
            # LV4 클러스터 통계
            if self.use_lv4:
                for cluster_id in range(1, max(self.clusters_lv4) + 1):
                    cluster_data = self.data_groups[self.data_groups['cluster_lv4'] == cluster_id]
                    
                    if len(cluster_data) > 0:
                        all_keywords = [kw for keywords in cluster_data['keyword_list'] for kw in keywords]
                        keyword_freq = pd.Series(all_keywords).value_counts()
                        
                        cluster_name = self.extract_unique_keywords(keyword_freq.to_dict())
                        self.data_groups.loc[self.data_groups['cluster_lv4'] == cluster_id, 'cluster_lv4_name'] = cluster_name
                        
                        cluster_stats['lv4'][cluster_id] = {
                            'size': len(cluster_data),
                            'name': cluster_name,
                            'common_keywords': keyword_freq.head(5).to_dict(),
                            'sample_texts': cluster_data['original_text'].head(3).tolist()
                        }
            
            logging.info("Cluster statistics generation completed")
            return cluster_stats
                
        except Exception as e:
            logging.error(f"Error in clustering: {str(e)}")
            raise
        
    def perform_clustering_k_medoids(self, n_clusters_lv3=20, n_clusters_lv4=None,  detect_outliers=False, outlier_factor=3.0):
        """K-medoids 2단계 군집화 수행 (선택적 이상치 탐지)"""
        try:
            cluster_stats = {
                'lv3': defaultdict(dict),
                'lv4': defaultdict(dict) if n_clusters_lv4 else None
            }

            # LV3 클러스터링
            '''
            distance_matrix = 1 - self.similarity_matrix
            kmedoids_lv3 = KMedoids(n_clusters=n_clusters_lv3, metric='precomputed', random_state=42)
            self.clusters_lv3 = kmedoids_lv3.fit_predict(distance_matrix) + 1
            '''
             # 거리 행렬 전처리
            distance_matrix = 1 - self.similarity_matrix
            
            # 거리 스케일링 적용
            from sklearn.preprocessing import MinMaxScaler
            scaler = MinMaxScaler()
            distance_matrix_scaled = scaler.fit_transform(distance_matrix)
            
            # K-medoids 파라미터 조정
            kmedoids_lv3 = KMedoids(
                n_clusters=n_clusters_lv3,
                metric='precomputed',
                init='k-medoids++',
                max_iter=1000,
                random_state=42
            )
            self.clusters_lv3 = kmedoids_lv3.fit_predict(distance_matrix_scaled) + 1
            
            if detect_outliers:
                # 각 데이터 포인트와 medoid 간의 거리 계산
                distances_to_medoids = np.min(kmedoids_lv3.transform(distance_matrix), axis=1)
                
                if self.threshold is not None:
                    # 명시적 threshold 사용
                    outlier_mask = distances_to_medoids > self.threshold
                else:
                    # IQR 방식으로 극단적 이상치만 탐지
                    Q1 = np.percentile(distances_to_medoids, 25)
                    Q3 = np.percentile(distances_to_medoids, 75)
                    IQR = Q3 - Q1
                    upper_bound = Q3 + outlier_factor * IQR
                    outlier_mask = distances_to_medoids > upper_bound
                
                if np.any(outlier_mask):
                    self.clusters_lv3[outlier_mask] = n_clusters_lv3 + 1
                    logging.info(f"Found {np.sum(outlier_mask)} outliers in LV3")
            
            self.data_groups['cluster_lv3'] = self.clusters_lv3
            logging.info(f"LV3 K-medoids clustering completed with {n_clusters_lv3} clusters")
            
            # LV4 클러스터링 (선택적)
            if n_clusters_lv4 is not None and n_clusters_lv4 > 0:
                self.use_lv4 = True
                self.clusters_lv4 = np.zeros_like(self.clusters_lv3)
                
                # 이상치를 제외한 클러스터에 대해서만 LV4 클러스터링 수행
                max_cluster_lv3 = n_clusters_lv3 + (1 if detect_outliers else 0)
                for lv3_cluster in range(1, max_cluster_lv3 + 1):
                    cluster_mask = self.data_groups['cluster_lv3'] == lv3_cluster
                    cluster_size = np.sum(cluster_mask)
                    
                    if cluster_size > n_clusters_lv4:
                        subset_indices = np.where(cluster_mask)[0]
                        subset_matrix = self.similarity_matrix[np.ix_(subset_indices, subset_indices)]
                        '''
                        subset_distance = 1 - subset_matrix
                        
                        # LV4 K-medoids 클러스터링
                        kmedoids_lv4 = KMedoids(n_clusters=n_clusters_lv4, metric='precomputed', random_state=42)
                        subset_clusters = kmedoids_lv4.fit_predict(subset_distance) + 1
                        '''
                         # 거리 행렬 전처리
                        subset_distance = 1 - subset_matrix
                        
                        # 거리 스케일링 적용
                        subset_distance_matrix_scaled = scaler.fit_transform(subset_distance)
                        
                        # K-medoids 파라미터 조정
                        kmedoids_lv4 = KMedoids(
                            n_clusters=n_clusters_lv4,
                            metric='precomputed',
                            init='k-medoids++',
                            max_iter=1000,
                            random_state=42
                        )
                        subset_clusters = kmedoids_lv4.fit_predict(subset_distance_matrix_scaled) + 1
                        
                        # LV4 이상치 탐지 (선택적)
                        if detect_outliers:
                            subset_distances = np.min(kmedoids_lv4.transform(subset_distance), axis=1)
                            
                            if self.threshold is not None:
                                subset_outliers = subset_distances > self.threshold
                            else:
                                Q1 = np.percentile(subset_distances, 25)
                                Q3 = np.percentile(subset_distances, 75)
                                IQR = Q3 - Q1
                                upper_bound = Q3 + outlier_factor * IQR
                                subset_outliers = subset_distances > upper_bound
                                
                            if np.any(subset_outliers):
                                subset_clusters[subset_outliers] = n_clusters_lv4 + 1
                                logging.info(f"Found {np.sum(subset_outliers)} outliers in LV4 cluster {lv3_cluster}")
                        
                        self.clusters_lv4[subset_indices] = lv3_cluster * 100 + subset_clusters
                    else:
                        self.clusters_lv4[cluster_mask] = lv3_cluster * 100
                
                self.data_groups['cluster_lv4'] = self.clusters_lv4
                logging.info("LV4 K-medoids clustering completed")
            else:
                logging.info("Skipping LV4 clustering")

            # LV3 클러스터 통계 및 이름 생성
            max_cluster_lv3 = max(self.clusters_lv3)
            for cluster_id in range(1, max_cluster_lv3 + 1):
                cluster_data = self.data_groups[self.data_groups['cluster_lv3'] == cluster_id]
                if len(cluster_data) > 0:  # 빈 클러스터 제외
                    all_keywords = [kw for keywords in cluster_data['keyword_list'] for kw in keywords]
                    keyword_freq = pd.Series(all_keywords).value_counts()
                    
                    # 클러스터 이름 생성
                    cluster_name = self.extract_unique_keywords(keyword_freq.to_dict())
                    
                    # 데이터프레임에 클러스터 이름 추가
                    self.data_groups.loc[self.data_groups['cluster_lv3'] == cluster_id, 'cluster_lv3_name'] = cluster_name
                    
                    cluster_stats['lv3'][cluster_id] = {
                        'size': len(cluster_data),
                        'name': cluster_name,
                        'common_keywords': keyword_freq.head(5).to_dict(),
                        'sample_texts': cluster_data['original_text'].head(3).tolist(),
                        'is_outlier': detect_outliers and (cluster_id == n_clusters_lv3 + 1)
                    }

            # LV4 클러스터 통계 및 이름 생성
            if self.use_lv4:
                max_cluster_lv4 = max(self.clusters_lv4)
                for cluster_id in range(1, max_cluster_lv4 + 1):
                    cluster_data = self.data_groups[self.data_groups['cluster_lv4'] == cluster_id]
                    if len(cluster_data) > 0:  # 빈 클러스터 제외
                        all_keywords = [kw for keywords in cluster_data['keyword_list'] for kw in keywords]
                        keyword_freq = pd.Series(all_keywords).value_counts()
                        
                        # 클러스터 이름 생성
                        cluster_name = self.extract_unique_keywords(keyword_freq.to_dict())
                        
                        # 데이터프레임에 클러스터 이름 추가
                        self.data_groups.loc[self.data_groups['cluster_lv4'] == cluster_id, 'cluster_lv4_name'] = cluster_name
                        
                        lv3_parent = cluster_id // 100
                        is_outlier = detect_outliers and (
                            lv3_parent == n_clusters_lv3 + 1 or 
                            cluster_id % 100 == n_clusters_lv4 + 1
                        )
                        
                        cluster_stats['lv4'][cluster_id] = {
                            'size': len(cluster_data),
                            'name': cluster_name,
                            'common_keywords': keyword_freq.head(5).to_dict(),
                            'sample_texts': cluster_data['original_text'].head(3).tolist(),
                            'is_outlier': is_outlier
                        }

            return cluster_stats
            
        except Exception as e:
            logging.error(f"Error in K-medoids clustering: {str(e)}")
            raise
                
    
    def extract_unique_keywords(self, keywords_freq: dict) -> str:
        """빈도수가 높은 순으로 중복되지 않는 키워드 3개 추출"""
        unique_keywords = []
        used_parts = set()
        
        for keyword, _ in keywords_freq.items():
            # 이미 사용된 부분 단어가 있는지 확인
            parts = keyword.split('_')
            is_unique = True
            
            for part in parts:
                if part in used_parts:
                    is_unique = False
                    break
            
            if is_unique:
                # 새로운 부분 단어들을 used_parts에 추가
                used_parts.update(parts)
                unique_keywords.append(keyword)
                
                if len(unique_keywords) == 3:
                    break
        
        return ' '.join(unique_keywords)
    
    def export_results_to_excel(self,n_cluster_lv3,n_cluster_lv4):
        """간소화된 Excel 파일 생성"""
        try:
            # 타임스탬프 추가
            timestamp = datetime.now().strftime('%Y%m%d_%H%M%S')
            output_file = f'output_{self.lv1}_{self.lv2}_{n_cluster_lv3}_{n_cluster_lv4}_{timestamp}.xlsx'
            
            # 필요한 컬럼만 선택
            result_df = self.data_groups[['data_id', 'original_text', 'cluster_lv3', 
                                        'cluster_lv3_name']]
            
            # LV4 클러스터링이 수행된 경우 해당 컬럼 추가
            if self.use_lv4:
                result_df = self.data_groups[['data_id', 'original_text', 'cluster_lv3', 
                                            'cluster_lv3_name', 'cluster_lv4', 'cluster_lv4_name']]
            
            # Excel 파일로 저장
            result_df.to_excel(output_file, index=False)
            logging.info(f"Results exported to {output_file}")
            
            return {
                'file_path': output_file,
                'data': result_df.to_dict('records')
            }
            
            #return output_file
            
        except Exception as e:
            logging.error(f"Error exporting results to Excel: {str(e)}")
            raise
            
    def detect_outliers(distances, factor=3.0):
        """
        IQR 방식으로 이상치 탐지
        factor: 이상치 판단 기준 (높을수록 극단적인 이상치만 탐지)
        """
        Q1 = np.percentile(distances, 25)
        Q3 = np.percentile(distances, 75)
        IQR = Q3 - Q1
        upper_bound = Q3 + factor * IQR
        return distances > upper_bound

    def visualize_clusters(self):
            """군집화 결과 시각화"""
            if self.use_lv4:
                # LV3와 LV4 모두 시각화
                fig = plt.figure(figsize=(20, 10))
                
                # LV3 시각화
                plt.subplot(2, 2, 1)
                plt.imshow(self.similarity_matrix, cmap='viridis')
                plt.colorbar()
                plt.title('Group Similarity Matrix')
                
                plt.subplot(2, 2, 2)
                cluster_sizes_lv3 = pd.Series(self.clusters_lv3).value_counts().sort_index()
                cluster_sizes_lv3.plot(kind='bar')
                plt.title('LV3 Cluster Sizes')
                plt.xlabel('Cluster ID')
                plt.ylabel('Number of Groups')
                
                # LV4 시각화
                plt.subplot(2, 2, 3)
                cluster_sizes_lv4 = pd.Series(self.clusters_lv4).value_counts().sort_index()
                cluster_sizes_lv4.plot(kind='bar')
                plt.title('LV4 Cluster Sizes')
                plt.xlabel('Cluster ID')
                plt.ylabel('Number of Groups')
                
            else:
                # LV3만 시각화
                plt.figure(figsize=(15, 10))
                
                plt.subplot(1, 2, 1)
                plt.imshow(self.similarity_matrix, cmap='viridis')
                plt.colorbar()
                plt.title('Group Similarity Matrix')
                
                plt.subplot(1, 2, 2)
                cluster_sizes = pd.Series(self.clusters_lv3).value_counts().sort_index()
                cluster_sizes.plot(kind='bar')
                plt.title('LV3 Cluster Sizes')
                plt.xlabel('Cluster ID')
                plt.ylabel('Number of Groups')
            
            plt.tight_layout()
            plt.show()

def main():
    try:
        start_time = time.time()
        
        # 임시 파일에서 기존 데이터 로드
        #temp_data_file = f'temp_data_{os.getpid()}.json'
        

        # 임시 파일 경로 설정
        #temp_data_file = os.path.join(tempfile.gettempdir(), f'temp_data_{os.getpid()}.json')
        temp_data_file = os.path.join(tempfile.gettempdir(), f'temp_data.json')
        all_data = []
        
        if os.path.exists(temp_data_file):
            try:
                with open(temp_data_file, 'r', encoding='utf-8') as f:
                    all_data = json.load(f)
                logging.info(f"Loaded {len(all_data)} existing records from temp file")
            except Exception as e:
                logging.error(f"Error loading temp file: {str(e)}")
                all_data = []

        # stderr를 임시 버퍼로 리다이렉트하여 tqdm 출력 숨기기
        with contextlib.redirect_stderr(io.StringIO()):
            # 새로운 데이터 읽기
            input_data = json.loads(sys.stdin.readline())
            logging.info("input_data print")
            logging.info(input_data)
            all_data.extend(input_data['Data'])
        
            # 임시 파일에 데이터 저장
            with open(temp_data_file, 'w', encoding='utf-8') as f:
                json.dump(all_data, f, ensure_ascii=False)
        
        # 마지막 배치가 아니면 "received" 상태만 반환
        if not input_data.get('IsLastBatch', False):
            result = {
                'status': 'received',
                'count': len(all_data)
            }
            #print(base64.b64encode(json.dumps(result).encode()).decode())
            encoded_result = base64.b64encode(json.dumps(result).encode('utf-8')).decode('utf-8')
            #sys.stdout.write(encoded_result)
            print(encoded_result)
            sys.stdout.flush()
            return
        
        # 마지막 배치일 경우 클러스터링 수행
        with contextlib.redirect_stderr(io.StringIO()):
            lv1 = input_data.get('Lv1')
            lv2 = input_data.get('Lv2')
            threshold = None
            sub_threshold = None

            if input_data.get('Threshold') != 0 :
                threshold = input_data.get('Threshold')
            if input_data.get('SubThreshold') != 0 :
                sub_threshold = input_data.get('SubThreshold')

            n_clusters_lv3 = input_data.get('NClustersLv3')
            n_clusters_lv4 = input_data.get('NClustersLv4')
            
            # 클러스터링 분석기 초기화
            analyzer = GroupClusterAnalyzer(lv1, lv2, threshold, sub_threshold)
            
            # 누적된 전체 데이터로 클러스터링 수행
            analyzer.prepare_data_from_json(all_data)
            analyzer.calculate_group_similarity_matrix()
            cluster_stats = analyzer.perform_clustering(
                n_clusters_lv3=n_clusters_lv3,
                n_clusters_lv4=n_clusters_lv4
            )
        
            # 결과 반환
            result = analyzer.export_results_to_excel(n_clusters_lv3, n_clusters_lv4)
            
            if os.path.exists(temp_data_file):
               os.remove(temp_data_file)   

            task_dir = f'task_lv1_{lv1}_lv2_{lv2}'
            if os.path.exists(task_dir):
                # 기존 태스크 디렉토리가 있으면 삭제
                shutil.rmtree(task_dir) 
        
        # Base64 인코딩하여 결과 반환
        encoded_result = base64.b64encode(json.dumps(result).encode('utf-8')).decode('utf-8')
        #print(encoded_result) 
        #sys.stdout.write(encoded_result)
        #sys.stdout.flush()
        # 로깅 추가
        logging.info(f"Encoded result length: {len(encoded_result)}")
        logging.info(f"First 50 chars of encoded result: {encoded_result[:50]}")
        
        print(encoded_result, flush=True)
        sys.stdout.flush()
        
        end_time = time.time()
        execution_time = end_time - start_time
        logging.info(f"Execution completed in {execution_time:.2f} seconds")

    except Exception as e:
        error_result = {
            'error': str(e),
            'stacktrace': traceback.format_exc()
        }
        #print(base64.b64encode(json.dumps(error_result).encode()).decode())
        encoded_result = base64.b64encode(json.dumps(error_result).encode('utf-8')).decode('utf-8')
        #sys.stdout.write(encoded_result)
        #sys.stdout.flush()
        # 에러 로깅 추가
        logging.error(f"Error occurred: {error_result}")
        
        # Base64 인코딩
        error_json = json.dumps(error_result)
        encoded_error = base64.b64encode(error_json.encode()).decode()
        
        print(encoded_error, flush=True)
        sys.stdout.flush()
        
        end_time = time.time()
        execution_time = end_time - start_time
        logging.info(f"Execution completed in {execution_time:.2f} seconds")
        sys.exit(1)

if __name__ == "__main__":
     main()