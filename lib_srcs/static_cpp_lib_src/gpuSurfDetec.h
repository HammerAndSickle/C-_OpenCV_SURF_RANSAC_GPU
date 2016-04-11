#include <cstdio>
#include <iostream>
#include <opencv2/core/core.hpp>
#include <opencv2/cudafeatures2d.hpp>
#include <opencv2/xfeatures2d/nonfree.hpp>
#include <opencv2/xfeatures2d.hpp>
#include <opencv2/highgui/highgui.hpp>
#include <opencv2/xfeatures2d/cuda.hpp>
#include <opencv2/calib3d/calib3d.hpp>

using namespace cv;
using namespace cv::cuda;

class GpuSurfDetectionClass
{
private:
	cv::String targetImg;

	//SURF CLASS
	SURF_CUDA surf_cuda;

	bool printCap;

	//GPU - 브루트 포스 매처 선언. 
	Ptr<cuda::DescriptorMatcher> mc;

	//target 이미지 - GPU
	GpuMat* GPU_img_object;
	//target 이미지 - Mat
	Mat img_object;
	//target 특징점, 디스크립터
	GpuMat* t_keypoints;
	GpuMat* t_descriptor;
	std::vector<KeyPoint>* t_keypoint_vec;
public:
	//검출된 영역
	int* recPts;
public:
	GpuSurfDetectionClass(char* targetFname, int SurfHessian = 400);
	~GpuSurfDetectionClass();
	void printCapOn();
	void doMatching(char* img_frame_Orig_arr, int rows, int cols);
};