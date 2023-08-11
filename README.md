# dcinside2csv

Converts HTML file which is from DCInside gallery post page into CSV files, which is compatible to WXR(Wordpress eXtended RSS).

디시인사이드 갤러리의 글과 댓글을 CSV 파일로 저장해줍니다.

# 실행 환경

* .NET 6 런타임 [**[다운로드](https://dotnet.microsoft.com/ko-kr/download/dotnet/6.0)**]

# 개발 환경

* .NET 6 SDK [[다운로드](https://dotnet.microsoft.com/ko-kr/download/dotnet/6.0)]
* Visual Studio 2022

# 사용법

1. "SingleFile" 크롬 확장 프로그램을 설치해주세요 [[Link](https://chrome.google.com/webstore/detail/mpiodijhokgodhhofbcjdecpffjipkle)]
2. SingleFile 확장 프로그램을 이용해 원하는 페이지들을 HTML 파일로 저장합니다
   (HTML 파일들은 한 폴더 안에 모아두세요)
3. 터미널을 열고 dcinside2csv 프로그램을 실행합니다
   (예: `dotnet dcinside2csv.dll -i "D:\temp\dcinside\gallery" -o "D:\temp\dcinside\gallery_csv" -h "https://YOURID.wordpress.com/" -f "https://YOURID.files.wordpress.com/"`)
4. `gallery_csv` 폴더에 결과물들이 저장됩니다
   1. `input.csv`: 게시글
   2. `comments/*.csv`: 게시글 별 댓글 (숫자는 게시글 번호입니다)
   3. `images/img-*.*.csv`: 게시글 별 이미지 (img-게시글번호-이미지번호)

이렇게 추출된 CSV 파일과 이미지 파일을 원하는 곳에서 사용하면 됩니다.
