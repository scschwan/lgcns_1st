import sys
import json
import traceback
import logging
import os
from datetime import datetime
from konlpy.tag import Okt
import io
import base64
from typing import List
import time


sys.stdin = io.TextIOWrapper(sys.stdin.buffer, encoding='utf-8')
sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding='utf-8')


class TextData:
   def __init__(self, row: int, col: int, text: str):
       self.row = row
       self.col = col
       self.text = text

class OktKeywordExtractor:
   def __init__(self):
       try:
           self.okt = Okt()
       except:
           print("Okt 초기화 실패")
           self.okt = None

def setup_logger():
   log_dir = "python_log"
   os.makedirs(log_dir, exist_ok=True)
   logger = logging.getLogger('KeywordExtractor')
   logger.setLevel(logging.DEBUG)
   handler = logging.FileHandler(os.path.join(log_dir, f'{datetime.now():%Y-%m-%d}.log'), encoding='utf-8')
   handler.setFormatter(logging.Formatter('[%(asctime)s] %(levelname)s [%(filename)s:%(lineno)d] - %(message)s'))
   logger.addHandler(handler)
   return logger

logger = setup_logger()

def process_text_with_limit(text_data: List[TextData], okt: Okt, limit: int) -> list:
   result = []
   #col_offset = 0

   # texts를 Row 기준으로 그룹화
   for row in sorted(set(t.row for t in text_data)):
       row_texts = [t for t in text_data if t.row == row]
       
       #row 초기화 마다 col_offset 초기화
       col_offset = 0
       for text in sorted(row_texts, key=lambda x: x.col):
           if len(text.text) < limit:
               result.append(TextData(text.row, text.col + col_offset, text.text))
               continue
               
           try:
               morphs = okt.morphs(text.text)
               for i, morph in enumerate(morphs):
                   result.append(TextData(text.row, text.col + col_offset + i, morph))
               col_offset += len(morphs) - 1
           except Exception as e:
               logger.error(f"형태소 분석 오류: {str(e)}")
               result.append(TextData(text.row, text.col + col_offset, text.text))

   return result

def main():
   
   logger.info("처리 시작")
   start_time = time.time()
   
   try:
       logger.info(f"stdin encoding: {sys.stdin.encoding}")
       logger.info(f"stdout encoding: {sys.stdout.encoding}")


       input_data = json.loads(sys.stdin.buffer.read().decode('utf-8'))

       logger.debug(f"입력 데이터: {input_data if input_data else 'None'}")


       texts = [TextData(t["Row"], t["Col"], t["Text"]) for t in input_data.get("Texts", [])]
       limit = input_data.get("Limit", 4)
       
       extractor = OktKeywordExtractor()
       if not extractor.okt:
           raise Exception("Okt 초기화 실패")

       # 기본적인 데이터 출력
       logger.debug(f"Number of texts: {len(texts)}")
       #for text in texts:
       #     logger.debug(f"Row: {text.row}, Col: {text.col}, Text: {text.text}")

       logger.debug(f"limit: {limit}")

       # 전체 텍스트 데이터를 한 번에 처리
       processed_data = process_text_with_limit(texts, extractor.okt, limit)
       #logger.debug(f"processed_data: {processed_data}")

       result = [{
           "Row": data.row,
           "Col": data.col,
           "Text": data.text
       } for data in processed_data]

       result_json = json.dumps(result, ensure_ascii=False) 

       logger.debug("결과 JSON 생성 완료")
       logger.debug(f"결과 길이: {len(result_json)}")
       logger.debug(f"결과 데이터 {result_json}")
       
       encoded_result = base64.b64encode(result_json.encode('utf-8')).decode('ascii')
       print(encoded_result.strip(), end='', flush=True)
       
       logger.debug(f"Base64 인코딩 결과: {encoded_result}")
       logger.debug(f"Base64결과 길이: {len(encoded_result)}")
       
       sys.stdout.flush()

       logger.info(f"처리 완료. 소요 시간: {time.time() - start_time:.2f}초")

   except Exception as e:
       logger.error(f"오류 발생: {str(e)}\n{traceback.format_exc()}")
       sys.exit(1)

   finally:
        # 로그 핸들러 정리
        for handler in logger.handlers[:]:
            handler.close()
            logger.removeHandler(handler)
        
        sys.exit(0)

if __name__ == "__main__":
   main()