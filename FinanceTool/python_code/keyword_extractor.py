import sys
import json
import traceback
import logging
import os
import time
from datetime import datetime
from krwordrank.word import KRWordRank

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
    console_handler.setFormatter(formatter)
    
    logger.addHandler(file_handler)
    logger.addHandler(console_handler)
    
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
    try:
        #logger.debug(f"입력 텍스트: {text[:200]}...")  # 처음 200자만 로깅
        
        words = process_text(text, separators)
        if not words or len(words) == 1:
            return words
            
        sentence = ' '.join(words)
        extractor = KRWordRank(min_count=min_count, max_length=max_length)
        
        try:
            keywords, rank, graph = extractor.extract(
                [sentence],
                beta=0.85,
                max_iter=10
            )
            
            if not keywords:
                return words
                
            keywords_list = sorted(keywords.items(), key=lambda x: x[1], reverse=True)
            return [word for word, _ in keywords_list]
            
        except ValueError as ve:
            if "graph should consist of at least two nodes" in str(ve):
                return words
            raise
            
    except Exception as e:
        logger.error(f"키워드 추출 오류: {str(e)}")
        return words

def main():
    logger = setup_logger()
    logger.info("키워드 추출 프로세스 시작")
    start_time = time.time()
    
    try:
        # 입력 데이터 읽기
        logger.debug("입력 데이터 읽기 시작")
        input_line = sys.stdin.readline()
        
        logger.debug(f"읽은 데이터 길이: {len(input_line) if input_line else 0}")
        logger.debug(f"읽은 데이터: {input_line[:200] if input_line else 'None'}")
        
        if not input_line:
            raise ValueError("입력이 비어있습니다")

        # JSON 파싱
        try:
            input_data = json.loads(input_line)
            logger.debug(f"파싱된 JSON 키: {list(input_data.keys())}")
        except json.JSONDecodeError as e:
            logger.error(f"JSON 파싱 오류: {str(e)}")
            raise ValueError(f"잘못된 JSON 입력: {str(e)}")

        texts = input_data.get("texts", [])
        separators = input_data.get("separators", [])
        
        if not texts:
            raise ValueError("처리할 텍스트가 없습니다")

        # 키워드 추출
        results = []
        for idx, text in enumerate(texts, 1):
            try:
                keywords = extract_keywords(text, logger, separators)
                results.append({"keywords": keywords})
            except Exception as e:
                logger.error(f"텍스트 {idx} 처리 중 오류: {str(e)}")
                results.append({"keywords": [], "error": str(e)})

        # 결과 생성
        response = {"results": results}
        result_json = json.dumps(response, ensure_ascii=False)
        
        logger.debug("결과 JSON 생성 완료")
        logger.debug(f"결과 길이: {len(result_json)}")

        # Base64로 인코딩
        import base64
        encoded_result = base64.b64encode(result_json.encode('utf-8')).decode('utf-8')
        
        # 결과 출력 및 처리 완료 로깅
        print(encoded_result)
        sys.stdout.flush()
        
        logger.info(f"처리 완료. 소요 시간: {time.time() - start_time:.2f}초")
        
    except Exception as e:
        logger.error(f"처리 중 오류 발생: {str(e)}")
        logger.error(traceback.format_exc())
        
        error_response = {
            "error": str(e),
            "traceback": traceback.format_exc()
        }
        error_json = json.dumps(error_response, ensure_ascii=False)
        sys.stderr.write(error_json)
        sys.stderr.write('\n')
        sys.stderr.flush()
        
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
