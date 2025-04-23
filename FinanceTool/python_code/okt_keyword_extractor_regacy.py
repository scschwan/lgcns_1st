import sys
import json
import traceback
import logging
import os
import time
from datetime import datetime
import re
from konlpy.tag import Okt
from transformers import BertTokenizer, BertForMaskedLM
import io


# UTF-8 인코딩 설정

# 명시적 인코딩 설정
sys.stdin = io.TextIOWrapper(sys.stdin.buffer, encoding='utf-8')
sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding='utf-8')

class OktKeywordExtractor:
    def __init__(self):
        """Okt 및 검증기 초기화"""
        try:
            self.Okt = Okt()
            #print("Okt 초기화 완료")
        except:
            print("Okt 초기화 실패")
            self.Okt = None
            
        # BERT 검증기 초기화
        try:
            self.tokenizer = BertTokenizer.from_pretrained('klue/bert-base')
            self.model = BertForMaskedLM.from_pretrained('klue/bert-base')
            self.model.eval()
            #print("BERT 검증기 초기화 완료")
        except:
            print("BERT 검증기 초기화 실패")
            self.model = None
            
    def is_completed_korean_char(self, char):
        """완성형 한글 여부 체크"""
        return bool(re.match('[가-힣]', char))

    def is_korean_consonant(self, char):
        """한글 자음인지 확인"""
        consonants = 'ㄱㄲㄴㄷㄸㄹㅁㅂㅃㅅㅆㅇㅈㅉㅊㅋㅌㅍㅎ'
        return char in consonants

    def combine_and_verify_words(self, words):
        """단어 결합 및 검증 로직"""
        if not words:
            return words
        
        result = []
        i = 0
        
        while i < len(words):
            cur_word = words[i]
            
            # 2자 이하 단어는 그대로 유지
            if len(cur_word) <= 2:
                result.append(cur_word)
                i += 1
                continue
                
            # 자음이 포함된 경우 분리하지 않음
            if any(self.is_korean_consonant(char) for char in cur_word):
                result.append(cur_word)
                i += 1
                continue
                
            # 일반적인 단어 처리
            result.append(cur_word)
            i += 1
        
        return result

def setup_logger():
    """로깅 설정"""
    log_dir = "python_log"
    os.makedirs(log_dir, exist_ok=True)
    
    today = datetime.now().strftime('%Y-%m-%d')
    log_file = os.path.join(log_dir, f'{today}.log')
    
    logger = logging.getLogger('KeywordExtractor')
    logger.setLevel(logging.DEBUG)
    
    # 기존 핸들러 제거
    logger.handlers.clear()
    
    # 파일 핸들러 설정
    file_handler = logging.FileHandler(log_file, encoding='utf-8')
    file_handler.setLevel(logging.DEBUG)
    
    # 콘솔 핸들러 설정
    console_handler = logging.StreamHandler()
    console_handler.setLevel(logging.ERROR)
    
    # 포맷터 설정
    formatter = logging.Formatter(
        '[%(asctime)s] %(levelname)s [%(filename)s:%(lineno)d] - %(message)s',
        datefmt='%Y-%m-%d %H:%M:%S'
    )
    file_handler.setFormatter(formatter)
    #console_handler.setFormatter(formatter)
    
    logger.addHandler(file_handler)
    #logger.addHandler(console_handler)
    
    return logger

def clean_text(text):
    """텍스트에서 유효한 문자만 추출"""
    if not text:
        return ""
    
    def is_valid_char(char):
        return (
            ('가' <= char <= '힣') or
            ('a' <= char <= 'z') or
            ('A' <= char <= 'Z') or
            ('0' <= char <= '9') or
            (char in ' .,()-_')
        )
    
    return ''.join(char for char in text if is_valid_char(char))

def process_text(text, separators):
    """텍스트 전처리 및 단어 분리"""
    if not text or len(text) < 2:
        return []
    
    processed_text = text
    for separator in separators:
        processed_text = processed_text.replace(separator, ' ')
    
    words = []
    for word in processed_text.split():
        cleaned_word = clean_text(word.strip())
        if cleaned_word and len(cleaned_word) > 1:
            words.append(cleaned_word)
    
    return words

def extract_keywords(text, logger, separators, min_count=1, max_length=10):
    """키워드 추출"""
    import warnings

    # warning 메시지 무시 설정
    warnings.filterwarnings('ignore', category=UserWarning)

    try:
        # 텍스트 전처리
        words = process_text(text, separators)
        if not words or len(words) == 1:
            return words
            
        # Okt 분석기 초기화 (싱글톤 패턴)
        global _extractor
        if '_extractor' not in globals():
            _extractor = OktKeywordExtractor()
            
        if not _extractor.Okt:
            logger.info("Okt 초기화 실패")  # 로깅 추가
            return words
            
        # Okt으로 형태소 분석
        try:
            morphs = _extractor.Okt.morphs(text)
        except Exception as e:
             logger.info(f"형태소 분석 중 오류: {str(e)}")
             return words
        
        # 형태소 결합 및 검증
        keywords = _extractor.combine_and_verify_words(morphs)
        
        # 중복 제거 및 길이 필터링
        filtered_keywords = []
        seen = set()
        for keyword in keywords:
            if (keyword not in seen and 
                len(keyword) >= min_count and 
                len(keyword) <= max_length):
                filtered_keywords.append(keyword)
                seen.add(keyword)
        
        return filtered_keywords
            
    except Exception as e:
        logger.info(f"키워드 추출 상세 오류: {str(e)}")
        logger.info(traceback.format_exc())
        return words

def main():
    logger = setup_logger()
    logger.info("키워드 추출 프로세스 시작")
    start_time = time.time()
    
    try:
        logger.info(f"stdin encoding: {sys.stdin.encoding}")
        logger.info(f"stdout encoding: {sys.stdout.encoding}")
        # 입력 데이터 읽기
        try:
            #input_line = sys.stdin.readline()
            input_line = sys.stdin.buffer.read().decode('utf-8-sig')  # 바이너리로 읽고 디코딩
            #logger.debug(f"입력 데이터: {input_line[:200] if input_line else 'None'}")

            #logger.debug(f"입력 데이터: {input_line[:200] if input_line else 'None'}")
            logger.debug(f"입력 데이터: {input_line if input_line else 'None'}")
        except Exception as e:
            logger.info(f"입력 읽기 오류: {str(e)}")
            raise
        
        if not input_line:
            logger.info("빈 입력")
            raise ValueError("입력이 비어있습니다")

        # JSON 파싱
        try:
            input_data = json.loads(input_line)
            texts = input_data.get("texts", [])
            separators = input_data.get("separators", [])
            
            if not texts:
                logger.info("texts 배열 비어있음")
                raise ValueError("처리할 텍스트가 없습니다")
                
            logger.debug(f"처리할 텍스트 수: {len(texts)}")
            logger.debug(f"구분자: {separators}")
            
        except json.JSONDecodeError as e:
            #logger.info(f"JSON 파싱 상세 오류: {str(e)}, 입력값: {input_line[:100]}")
            logger.info(f"JSON 파싱 상세 오류: {str(e)}, 입력값: {input_line}")
            raise

        # 키워드 추출
        results = []
        for idx, text in enumerate(texts, 1):
            try:
                #logger.info(f"origin_text : {text}")
                keywords = extract_keywords(text, logger, separators)
                results.append({"keywords": keywords})
                #logger.info(f"keywords : {keywords}")
            except Exception as e:
                logger.info(f"텍스트 {idx} 처리 중 오류: {str(e)}")
                results.append({"keywords": [], "error": str(e)})

        # 결과 생성
        response = {"results": results}
        result_json = json.dumps(response, ensure_ascii=False)
        
        logger.debug("결과 JSON 생성 완료")
        logger.debug(f"결과 길이: {len(result_json)}")
        logger.debug(f"결과 데이터 {result_json}")
        # Base64로 인코딩
        import base64
        #encoded_result = base64.b64encode(result_json.encode('utf-8')).decode('utf-8')
        #print(encoded_result)

        # 문자열을 바이트로 변환 후 Base64 인코딩
        # Base64로 인코딩
        encoded_result = base64.b64encode(result_json.encode('utf-8')).decode('ascii')
        # 단일 라인으로 출력, 앞뒤 공백 없이
        print(encoded_result.strip(), end='', flush=True)
        
        # 결과 출력 및 처리 완료 로깅
        # 디버깅을 위한 로그
        logger.debug(f"Base64 인코딩 결과: {encoded_result}")
        logger.debug(f"Base64결과 길이: {len(encoded_result)}")
        
        
        #print(encoded_result)
        sys.stdout.flush()
        
        logger.info(f"처리 완료. 소요 시간: {time.time() - start_time:.2f}초")
        
    except Exception as e:
        logger.info(f"처리 중 오류 발생: {str(e)}")
        logger.info(traceback.format_exc())
        
        #error_response = {
        #    "error": str(e),
        #    "traceback": traceback.format_exc()
        #}
        #error_json = json.dumps(error_response, ensure_ascii=False)
        #sys.stderr.write(error_json)
        #sys.stderr.write('\n')
        #sys.stderr.flush()
        
        exit_code = 1
    else:
        exit_code = 0
    finally:
        # 로그 핸들러 정리
        for handler in logger.handlers[:]:
            handler.close()
            logger.removeHandler(handler)
        
        sys.exit(exit_code)

if __name__ == "__main__":
    main()
