pyinstaller --onefile --noconsole --hidden-import=konlpy --hidden-import=konlpy.tag --hidden-import=konlpy.tag._okt --hidden-import=konlpy.utils --hidden-import=typing --collect-all konlpy okt_keyword_extractor.py

pause