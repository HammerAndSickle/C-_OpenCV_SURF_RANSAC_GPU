SURF+RANSAC OBJECT DETECTION IN C#(OpenCVSharp)

필요 환경 : C#를 위한 .NET framework, OpenCV 3.1 dll(with CUDA, nonfree modules), OpenCVSharp 3.1, Webcam

CudaSurfDetection.dll을 참조에 추가시킨 후, C# 내에서 CudaSurfDetection.CudaSurfWrap 클래스를 선언 후,
C++/CLI를 이용하여, 이미지와 파일명을 C++로 보내 전송한 후, 그곳에서 검출을 끝내면 그 영역 좌표 4개를 받아와 출력

* C# OpenCVSharp를 통해 웹캠의 사진을 Mat으로 얻어온 후, 검출할 오브젝트 Template 이미지의 파일명과 함께 CudaSurfWrap 클래스에 바이트 배열로 전송
* CudaSurfWrap 클래스에서 검출을 시작, 바이트 배열로 전송된 웹캠 사진을 C++에서 받은 후, OpenCV 3.1+GPU를 통해 SURF detection 수행
* C++ 에서 추출해낸 검출 영역(4개의 (x, y) 포인트)을 C#으로 전송하여 C#에서 영역을 표시