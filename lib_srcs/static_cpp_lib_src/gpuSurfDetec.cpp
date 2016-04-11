#include "stdafx.h"
#include "gpuSurfDetec.h"

GpuSurfDetectionClass::GpuSurfDetectionClass(char* targetFname, int SurfHessian)
{
	recPts = new int[8];
	printCap = false;

	mc = cuda::DescriptorMatcher::createBFMatcher(surf_cuda.defaultNorm());
	t_keypoint_vec = new std::vector<KeyPoint>();

	GPU_img_object = new GpuMat();
	t_keypoints = new GpuMat();
	t_descriptor = new GpuMat();

	//target �̹����� �ε� ��, Ư¡��/��ũ���� ����
	img_object = imread(targetFname, CV_LOAD_IMAGE_GRAYSCALE);
	GPU_img_object->upload(img_object);	//mat -> GPUMAT

	surf_cuda(*GPU_img_object, GpuMat(), *t_keypoints, *t_descriptor);
	surf_cuda.downloadKeypoints(*t_keypoints, *t_keypoint_vec);

	//Hessian �Ӱ谪
	surf_cuda.hessianThreshold = SurfHessian;

}
void GpuSurfDetectionClass::printCapOn()
{
	if (printCap) printCap = false;

	else if (!printCap) printCap = true;
}

GpuSurfDetectionClass::~GpuSurfDetectionClass()
{
	delete[] recPts;
	delete GPU_img_object;
	delete t_keypoints;
	delete t_descriptor;
	delete t_keypoint_vec;
}
void GpuSurfDetectionClass::doMatching(char* img_frame_Orig_arr, int rows, int cols)
{
	Mat img_frame = cv::Mat(rows, cols, CV_8UC1, img_frame_Orig_arr);;

	//�ϳ��� ��Ī�� ���� ���� ������� �ٲ۴�.
	//cvtColor(img_frame_Orig, img_frame, CV_BGR2GRAY);

	//GPU - ī�޶�� ���� �̹���
	GpuMat* GPU_img_frame = new GpuMat();
	GPU_img_frame->upload(img_frame);

	//GPU - ī�޶� �̹����� SURF Ư¡��, ��ũ����
	GpuMat* f_keypoint = new GpuMat();
	GpuMat* f_descriptor = new GpuMat();

	//GPU - ī�޶� �̹����� SURF ����
	surf_cuda(*GPU_img_frame, GpuMat(), *f_keypoint, *f_descriptor);



	//ī�޶� �̹����� Ư¡���� ����. vector<float>���δٰ�.
	std::vector<KeyPoint>* f_keypoint_vec = new std::vector<KeyPoint>();
	surf_cuda.downloadKeypoints(*f_keypoint, *f_keypoint_vec);

	std::vector<DMatch> matches;

	//GPU - ��Ī
	mc->match(*t_descriptor, *f_descriptor, matches);



	//������ f_keypoint_vec�� t_keypoint_vec���� RANSAC�� �����Ѵ�.
	double max_dist = 0; double min_dist = 100;

	//printf("t_desc : %d\n", t_descriptors.rows);

	//-- Quick calculation of max and min distances between keypoints
	for (int i = 0; i < t_descriptor->rows; i++)
	{

		double dist = matches[i].distance;
		if (dist < min_dist) min_dist = dist;
		if (dist > max_dist) max_dist = dist;
	}

	printf("-- Max dist : %f \n", max_dist);
	printf("-- Min dist : %f \n", min_dist);

	//-- Draw only "good" matches (i.e. whose distance is less than 3*min_dist )
	std::vector< DMatch > good_matches;

	for (int i = 0; i < t_descriptor->rows; i++)
	{
		if (matches[i].distance < 3 * min_dist)
		{
			good_matches.push_back(matches[i]);
		}
	}


	//Mat img_matches;


	//drawMatches(img_object, keypoints_object, img_scene, keypoints_scene,
	//good_matches, img_matches, Scalar::all(-1), Scalar::all(-1),
	//vector<char>(), DrawMatchesFlags::NOT_DRAW_SINGLE_POINTS);

	if (good_matches.size() > 0) {

		//���� ��Ī������ ã�� �Ŀ� 'target' �� 'frame'���� ���� ����
		std::vector<Point2f> targetPts;
		std::vector<Point2f> framePts;

		for (int i = 0; i < good_matches.size(); i++)
		{
			//-- Get the keypoints from the good matches
			targetPts.push_back(t_keypoint_vec->at(good_matches[i].queryIdx).pt);
			framePts.push_back(f_keypoint_vec->at(good_matches[i].trainIdx).pt);
		}



		//RANSAC�� �̿��� homography ����� ã�´�.
		Mat Homo = findHomography(targetPts, framePts, CV_RANSAC);

		//-- Get the corners from the image_1 ( the object to be "detected" )
		std::vector<Point2f> obj_corners(4);
		obj_corners[0] = cvPoint(0, 0); obj_corners[1] = cvPoint(img_object.cols, 0);
		obj_corners[2] = cvPoint(img_object.cols, img_object.rows); obj_corners[3] = cvPoint(0, img_object.rows);
		std::vector<Point2f> scene_corners(4);

		//std::cout << H << std::endl;

		try{
			perspectiveTransform(obj_corners, scene_corners, Homo);
		}
		catch (cv::Exception)
		{
			return;
		}

		//����� ������ ����
		for (int i = 0; i < 4; i++)
		{
			recPts[2 * i] = scene_corners[i].x;
			recPts[2 * i + 1] = scene_corners[i].y;
		}

		if (printCap)
		{
			line(img_frame, scene_corners[0], scene_corners[1], Scalar(0, 255, 0), 4);
			line(img_frame, scene_corners[1], scene_corners[2], Scalar(0, 255, 0), 4);
			line(img_frame, scene_corners[2], scene_corners[3], Scalar(0, 255, 0), 4);
			line(img_frame, scene_corners[3], scene_corners[0], Scalar(0, 255, 0), 4);
		}
	}


	delete f_keypoint_vec;
	delete GPU_img_frame;
	delete f_descriptor;
	delete f_keypoint;

	if(printCap) imshow("Good Matches & Object detection", img_frame);

	return;

}