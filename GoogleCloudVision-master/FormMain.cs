
using Google.Apis.Auth.OAuth2;
using Google.Apis.Vision.v1.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;

//레퍼런스
//https://cloud.google.com/vision/
//구글 테스트 페이지
//https://cloud.google.com/vision/
//참조
// - 조대협
//http://bcho.tistory.com/1075
//http://blog.danggun.net/4621
//https://keestalkstech.com/2016/06/get-keywords-for-images-from-the-google-cloud-vision-api-with-c/
//http://googlecloudplatform.github.io/google-cloud-dotnet/docs/Google.Cloud.Vision.V1/
//http://www.buildinsider.net/web/bigquery/spinoff1
//http://dpdpwl.tistory.com/6
//http://bcho.tistory.com/1075
//http://www.hardcopyworld.com/gnuboard5/bbs/board.php?bo_table=lecture_rpi&wr_id=68
//http://www.hardcopyworld.com/ngine/aduino/index.php/archives/2736
namespace GoogleCloudVision_master
{
    public partial class FormMain : Form
    {

        //구글 클라우드 비전 서비스 파일
        string GoogleCloudVisionService_FileName = "GoogleCloudVision_service.json";
        string GoogleCloudVisionService_FilePath;
        //테스트 이미지 경로
        string TestImage_FileName = "img.png";
        string TestImage_FilePath;


        public FormMain()
        {
            InitializeComponent();           
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            try
            {   
                //구글 클라우드 비전 서비스 파일경로
                // - (실행파일 경로+구글 클라우드 서비스 파일 이름)
                this.GoogleCloudVisionService_FilePath = string.Format("{0}\\{1}", Application.StartupPath, this.GoogleCloudVisionService_FileName);
                FileInfo _finfo;
                _finfo = new FileInfo(GoogleCloudVisionService_FilePath);
                //파일 있는지 확인 있을때(true), 없으면(false)
                if (!_finfo.Exists)
                {
                    //블로그 포스팅 방법에 따라 구글 API 사용자 인증 정보 생성하여, 경로에 같은 이름으로 저장되어 있어야 한다.
                    //https://backeres.blogspot.com
                    MessageBox.Show("구글 클라우드 서비스 인증 파일이 존재하지 않습니다.\r\n경로에 파일을 저장 하십시오.");
                }


                //분석 할 이미지 파일 경로
                this.TestImage_FilePath = string.Format("{0}\\{1}", Application.StartupPath, this.TestImage_FileName);
                _finfo = new FileInfo(TestImage_FilePath);
                //파일 있는지 확인 있을때(true), 없으면(false)
                if (!_finfo.Exists)
                {                    
                    MessageBox.Show("구글 클라우드 비전 테스트 이미지가 경로에 존재 하지 않습니다.\r\n경로에 파일을 저장 하십시오.");
                }                
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());                
            }
            
        }

        public static GoogleCredential CreateCredentials(string path)
        {
            GoogleCredential credential;
            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                var c = GoogleCredential.FromStream(stream);
                credential = c.CreateScoped(Google.Apis.Vision.v1.VisionService.Scope.CloudPlatform);
            }

            return credential;
        }

        private void btnImg_Click(object sender, EventArgs e)
        {
                        
            try {

                //1.
                //다운받은 '사용자 서비스 키'를 지정하여 자격 증명을 초기화 한다.
                //구글 api 자격증명
                GoogleCredential credential =null;
                //GoogleCredential credential = CreateCredentials("GoogleCloudVision_service.json");                
                using (var stream = new FileStream(this.GoogleCloudVisionService_FilePath, FileMode.Open, FileAccess.Read))                
                {                    
                    string[] scopes = { Google.Apis.Vision.v1.VisionService.Scope.CloudPlatform };
                    credential = GoogleCredential.FromStream(stream);
                    credential = credential.CreateScoped(scopes);
                }
                   
                //2.             
                //자격증명을 가지고 구글 비전 서비스를 생성 한다.
                var service = new Google.Apis.Vision.v1.VisionService(new Google.Apis.Services.BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = "GoogleCloudVision-master",
                    GZipEnabled = true,
                });
                service.HttpClient.Timeout = new TimeSpan(1, 1, 1);

                //3.
                //분석 할 이미지를 읽어 온다.     
                byte[] file = File.ReadAllBytes(this.TestImage_FilePath);


                //4.
                /*
                [Feature type]
                LABEL_DETECTION: 이미지에 포함된 사물 인식
                FACE_DETECTION: 얼굴 인식
                TEXT_DETECTION: 문자 인식 (OCR, Optical Character Recognition)
                LANDMARK_DETECTION : 지형, 지물 인식
                LOGO_DETECTION: 회사 로고 인식
                SAFE_SEARCH_DETECTION : 19금 이미지 인식
                IMAGE_PROPERTIES : 이미지의 주요 특성 인식 (주요 색상 등)*/
                //분석 요청 생성
                BatchAnnotateImagesRequest batchRequest = new BatchAnnotateImagesRequest();
                batchRequest.Requests = new List<AnnotateImageRequest>();
                batchRequest.Requests.Add(new AnnotateImageRequest()
                {                    
                    //추출 타입을 하나 이상 초기화 한다.
                    Features = new List<Feature>() {
                        new Feature()
                        {
                            //라벨
                            Type = "LABEL_DETECTION",                            
                            MaxResults = 1
                        },
                        new Feature()
                        {                            
                            //텍스트
                            Type = "TEXT_DETECTION",                         
                            MaxResults = 1
                        },
                        new Feature()
                        {                            
                            //웹
                            Type = "WEB_DETECTION",
                            MaxResults = 1
                        },
                         new Feature()
                        {                            
                            //얼굴
                            Type = "FACE_DETECTION",
                            MaxResults = 1
                        },
                    },
                    ImageContext = new ImageContext() {
                        //감지할 언어를 설정하는 코드
                        LanguageHints = new List<string>() {
                            "en","ko"
                        }
                    },
                    Image = new Google.Apis.Vision.v1.Data.Image() { Content = Convert.ToBase64String(file) }
                });


                //요청 결과 받기
                var annotate = service.Images.Annotate(batchRequest);                
                BatchAnnotateImagesResponse batchAnnotateImagesResponse = annotate.Execute();
                if (batchAnnotateImagesResponse.Responses.Any())
                {
                    AnnotateImageResponse annotateImageResponse = batchAnnotateImagesResponse.Responses[0];
                    if (annotateImageResponse.Error != null)
                    {
                        //에러
                        if (annotateImageResponse.Error.Message != null)
                        {
                            richTextBox1.Text = annotateImageResponse.Error.Message;
                            MessageBox.Show("예러 : "+ richTextBox1.Text);                            
                        }
                            
                    }
                    else
                    {
                        /*
                        ///////////////////////////////////////////////////////////////////////////////////////////////////////                        
                        //요소를 직접 접근하여 출력
                        //텍스트 인식
                        richrichTex1.Text += "텍스트\r\n";
                        richTextBox1.Text += annotateImageResponse.TextAnnotations[0].Description.Replace("\n", "\r\n")+"\r\n";
                        richTextBox1.Text += "웹 디텍션\r\n";
                        richTextBox1.Text += annotateImageResponse.WebDetection.WebEntities[0].Description.Replace("\n", "\r\n") + "\r\n";

                        ///////////////////////////////////////////////////////////////////////////////////////////////////////
                        //얼굴 인식
                        richTextBox1.Text += "얼굴\r\n";
                        //[얼굴 주위의 경계 다각형]
                        //경계 상자의 좌표는 ImageParams에서 반환되는 원본 이미지의 눈금에 있습니다. 
                        //경계 상자는 인간의 기대에 따라 얼굴을 "프레임"하기 위해 계산됩니다. 
                        //랜드 마크 결과를 기반으로합니다. 
                        //주석 처리 할 이미지에 부분 면만 나타나면 BoundingPoly에서 
                        //하나 이상의 x 및 / 또는 y 좌표가 생성되지 않을 수 있습니다 (다각형은 제한되지 않음).
                        richTextBox1.Text += annotateImageResponse.FaceAnnotations[0].BoundingPoly.Vertices.Count + "\r\n";
                        //[피부의 양]
                        //fdBounding 폴리 경계 폴리곤은 boundingPoly보다 더 엄격하고, 
                        //얼굴의 스킨 부분 만 둘러 쌉니다. 
                        //일반적으로 이미지에서 볼 수있는 "피부의 양"을 감지하는 이미지 분석에서 얼굴을 제거하는 데 사용됩니다. 
                        //랜드 마커 결과를 기반으로하지 않으며 초기 얼굴 인식에서만 사용되므로
                        richTextBox1.Text += annotateImageResponse.FaceAnnotations[0].FdBoundingPoly.Vertices.Count + "\r\n";
                        //[갈정]
                        richTextBox1.Text += annotateImageResponse.FaceAnnotations[0].UnderExposedLikelihood + "\r\n";                        
                        richTextBox1.Text += annotateImageResponse.FaceAnnotations[0].AngerLikelihood + "\r\n";
                        richTextBox1.Text += annotateImageResponse.FaceAnnotations[0].JoyLikelihood + "\r\n";
                        richTextBox1.Text += annotateImageResponse.FaceAnnotations[0].SorrowLikelihood + "\r\n";
                        richTextBox1.Text += annotateImageResponse.FaceAnnotations[0].SurpriseLikelihood + "\r\n";
                        //[랜드마크]
                        richTextBox1.Text += annotateImageResponse.FaceAnnotations[0].LandmarkingConfidence + "\r\n";
                        richTextBox1.Text += annotateImageResponse.FaceAnnotations[0].Landmarks[0].Type + "\r\n";
                        richTextBox1.Text += annotateImageResponse.FaceAnnotations[0].Landmarks[0].Position + "\r\n";
                        //[앵글]
                        richTextBox1.Text += annotateImageResponse.FaceAnnotations[0].PanAngle + "\r\n";
                        richTextBox1.Text += annotateImageResponse.FaceAnnotations[0].RollAngle + "\r\n";                        
                        richTextBox1.Text += annotateImageResponse.FaceAnnotations[0].TiltAngle + "\r\n";
                        //[탐지 신뢰범위[0,1]]
                        richTextBox1.Text += annotateImageResponse.FaceAnnotations[0].DetectionConfidence + "\r\n";                        
                        ///////////////////////////////////////////////////////////////////////////////////////////////////////
                        //라벨
                        richTextBox1.Text += "라벨\r\n";
                        richTextBox1.Text += annotateImageResponse.LabelAnnotations[0].Mid.Replace("\n", "\r\n") + "\r\n";
                        richTextBox1.Text += annotateImageResponse.LabelAnnotations[0].Description.Replace("\n", "\r\n")+"\r\n";
                        richTextBox1.Text += annotateImageResponse.LabelAnnotations[0].Score + "\r\n";
                        //Console.WriteLine(richTextBox1.Text);
                        MessageBox.Show("성공 : " + richTextBox1.Text);*/

                        /*
                        //for 이용하여 출력
                        int count = annotateImageResponse.FaceAnnotations.Count;
                        foreach (var faceAnnotation in annotateImageResponse.FaceAnnotations)
                        {
                            Console.WriteLine("Face {0}:", count++);
                            Console.WriteLine("  Joy: {0}", faceAnnotation.JoyLikelihood);
                            Console.WriteLine("  Anger: {0}", faceAnnotation.AngerLikelihood);
                            Console.WriteLine("  Sorrow: {0}", faceAnnotation.SorrowLikelihood);
                            Console.WriteLine("  Surprise: {0}", faceAnnotation.SurpriseLikelihood);
                        }*/



                        //J객체로 변환하여 출력 한다.
                        JObject objJsonText = JObject.FromObject(annotateImageResponse);
                        //전체 데이터 출력
                        //richrichTextBox1.Text += "annotateImageResponse\r\n";
                        //richrichTextBox1.Text += objJsonText.ToString();             
                        //Console.WriteLine("JsonText :", objJsonText.ToString());
                        //항목 데이터
                        richTextBox1.Text += "labelAnnotations\r\n";
                        richTextBox1.Text += objJsonText["labelAnnotations"].ToString();
                        //항목 데이터
                        richTextBox1.Text += "faceAnnotations\r\n";
                        richTextBox1.Text += objJsonText["faceAnnotations"].ToString();
                        //항목 데이터
                        richTextBox1.Text += "webDetection\r\n";
                        richTextBox1.Text += objJsonText["webDetection"].ToString();                                                                
                        
                    }
                }

            }
            catch (Exception ex) {
                MessageBox.Show(ex.ToString());
            }            
        }

    }
}
