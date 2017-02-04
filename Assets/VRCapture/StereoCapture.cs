using UnityEngine;
using System.Collections;
using System.IO;

public class StereoCapture : MonoBehaviour
{
    const float HorizontalAngularIncrement = 1.0f;
    const float VerticalAngularIncrement = 90.0f;
    const float EyeSeparation = 0.032f;
    const float NearClipPlane = 0.1f;
    const int CaptureWidth = 2048;
    const int CaptureHeight = 1024;
    const int CaptureQuality = 75;
    const int BuffFrame = 2;

    public Material BiltMaterial;
    public string CaptureFolder = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments) + "/VRCaptures";
    public string EnableEffects;
    public int StartFrame = 0;
    public int EndFrame = -1;
    public int CaptureFrameRate = 60;
    public bool CaptureStart;

    int mFrameIdx = 0;
    Camera mCaptureCamera;
    GameObject mCameraRoot;
    RenderFrame[] mRenderFrames;
    Texture2D mResolveTexture;

    class RenderFrame
    {
        public RenderTexture RenderTargetLeft;
        public RenderTexture RenderTargetRight;
        public Texture2D ResolveTexture;
        public int FrameCount;
    }

    // Use this for initialization
    void Start()
    {
    }

    void OnDestroy()
    {
        if (mCameraRoot)
            Destroy(mCameraRoot);
        mCameraRoot = null;

        // we share the same texture.
        if (mResolveTexture)
            GameObject.Destroy(mResolveTexture);
        mResolveTexture = null;

        if (mRenderFrames != null)
        {
            foreach (var frame in mRenderFrames)
            {
                GameObject.Destroy(frame.RenderTargetLeft);
                GameObject.Destroy(frame.RenderTargetRight);
            }
            mRenderFrames = null;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (CaptureStart)
            CaptureFrame();
    }

    void StartCapture(int cubeSize, int fps)
    {
        CaptureStart = true;
    }

    void StopCapture()
    {
        CaptureStart = false;
    }

    void InitCapture()
    {
        //  alreay initialized.
        if (mCameraRoot)
            return;

        // capture started.
        mCameraRoot = new GameObject("CameraRoot");
        mCaptureCamera = mCameraRoot.AddComponent<Camera>();
        mCaptureCamera.CopyFrom(Camera.main);
        mCaptureCamera.nearClipPlane = NearClipPlane;

        // setup the capture mode.
        Time.captureFramerate = CaptureFrameRate;

        mResolveTexture = new Texture2D(CaptureWidth, CaptureHeight * 2, TextureFormat.ARGB32, false);

        // new the buffers.
        mRenderFrames = new RenderFrame[BuffFrame];
        for (var i = 0; i < BuffFrame; i++)
        {
            mRenderFrames[i] = new RenderFrame() {
                RenderTargetLeft = new RenderTexture(CaptureWidth, CaptureHeight, 0),
                RenderTargetRight = new RenderTexture(CaptureWidth, CaptureHeight, 0),
                ResolveTexture = mResolveTexture,
                FrameCount = -1
            };
        }
    }

    void CaptureFrame()
    {
        var frameCount = Time.frameCount;
        if (frameCount < StartFrame || (EndFrame >= 0 && frameCount > EndFrame + 1))
            return;

        InitCapture();

        // dump current frame (rendered).
        var curFrame = mRenderFrames[(mFrameIdx++) % mRenderFrames.Length];
        if (curFrame.FrameCount > 0)
        {
            DumpFrame(curFrame,
                CaptureFolder,
                CaptureQuality,
                false);
        }

        // capture to next frame.
        curFrame.FrameCount = frameCount;
        CaptureFrame(curFrame,
            CaptureWidth,
            CaptureHeight,
            HorizontalAngularIncrement,
            VerticalAngularIncrement,
            EyeSeparation,
            EnableEffects,
            mCaptureCamera,
            transform.position,
            BiltMaterial);
    }

    static void DumpFrame(
        RenderFrame frame,
        string captureFolder,
        int quality,
        bool losslessPNG)
    {
        // dump the left.
        RenderTexture.active = frame.RenderTargetLeft;
        frame.ResolveTexture.ReadPixels(new Rect(0, 0, frame.RenderTargetLeft.width, frame.RenderTargetLeft.height), 0, 0);

        // dump the right.
        RenderTexture.active = frame.RenderTargetRight;
        frame.ResolveTexture.ReadPixels(new Rect(0, 0, frame.RenderTargetRight.width, frame.RenderTargetRight.height), 0, frame.RenderTargetLeft.height);

        RenderTexture.active = null;
        frame.ResolveTexture.Apply();
        if (!string.IsNullOrEmpty(captureFolder))
        {
            // write to file.
            var fileName = string.Format("frame_{0}.{1}", frame.FrameCount.ToString("D5"), losslessPNG ? "png" : "jpg");
            File.WriteAllBytes(
                Path.Combine(captureFolder, fileName),
                losslessPNG ? frame.ResolveTexture.EncodeToPNG() : frame.ResolveTexture.EncodeToJPG(quality));
        }
    }

    static void CaptureFrame(
        RenderFrame frame,
        int captureWidth,
        int captureHeight,
        float horizontalInc,
        float verticalInc,
        float eyeSeparation,
        string effects,
        Camera camera,
        Vector3 capturePos,
        Material biltMaterial)
    {
        var cameraTrans = camera.transform;
        var horizontalSteps = (int)(360.0f / horizontalInc);
        var verticalSteps = (int)(180.0f / verticalInc + 1);

        camera.fieldOfView = 180.0f / verticalSteps;
        biltMaterial.SetVector("_BlitParams", new Vector4(
            1.0f / captureWidth,
            1.0f / captureHeight,
            horizontalSteps,
            verticalSteps));

        for (var i = 0; i < 2; i++)
        {
            var left = (i == 0);
            var eyeOffset = new Vector3(left ? -eyeSeparation : eyeSeparation, 0, 0);

            var renderTarget = RenderTexture.GetTemporary(captureWidth * 2, captureHeight * 2, 16, RenderTextureFormat.ARGB32);
            var sliderWidth = (float)renderTarget.width / horizontalSteps;
            var sliderHeight = (float)renderTarget.height / verticalSteps;

            // render the slides to the target.
            RenderTexture.active = renderTarget;
            camera.targetTexture = renderTarget;
            for (var row = 0; row < verticalSteps; row++)
            {
                for (var col = 0; col < horizontalSteps; col++)
                {
                    cameraTrans.rotation = Quaternion.Euler(
                        180.0f * ((verticalSteps / 2) - row) / verticalSteps,
                        180.0f + 360.0f * col / horizontalSteps,
                        0);
                    // TODO: Make rotation based on attached gameobject
                    cameraTrans.position = capturePos + cameraTrans.rotation * eyeOffset;
                    // build the viewport.
                    var rect = new Rect(col * sliderWidth, row * sliderHeight, sliderWidth, sliderHeight);
                    camera.pixelRect = rect;
                    camera.Render();
                }
            }
            camera.targetTexture = null;

            // slice to SphericalAtlas
            var sphericalAtlas = RenderTexture.GetTemporary(captureWidth, captureHeight);
            Graphics.Blit(renderTarget, sphericalAtlas, biltMaterial);
            RenderTexture.ReleaseTemporary(renderTarget);

            // process the full screen effects.
            PostProcessEffects(ref sphericalAtlas, effects);

            // read the pixels to texture..
            RenderTexture.active = sphericalAtlas;
            Graphics.Blit(sphericalAtlas, left ? frame.RenderTargetLeft : frame.RenderTargetRight);

            // no need.
            RenderTexture.active = null;
            RenderTexture.ReleaseTemporary(sphericalAtlas);
        }
    }

    public static void PostProcessEffects(ref RenderTexture src, string componentNames)
    {
        RenderTexture des = null;
        var components = componentNames.Split(" ;,".ToCharArray(), System.StringSplitOptions.RemoveEmptyEntries);
        for (var i = 0; i < components.Length; i++)
        {
            var component = Camera.main.GetComponent(components[i]);
            if (component == null)
                continue;

            var renderMethod = component.GetType().GetMethod("OnRenderImage");
            if (renderMethod == null)
                continue;

            if (!des) des = RenderTexture.GetTemporary(src.width, src.height);

            renderMethod.Invoke(component, new object[] { src, des });
            var tmp = des;
            des = src;
            src = tmp;
        }

        if (des)
            RenderTexture.ReleaseTemporary(des);
    }

    [ContextMenu("Capture")]
    void ExecuteCapture()
    {
        var cameraRoot = new GameObject("CameraRoot");
        var camera = cameraRoot.AddComponent<Camera>();
        camera.CopyFrom(Camera.main);
        camera.nearClipPlane = NearClipPlane;

        var frame = new RenderFrame() {
            RenderTargetLeft = new RenderTexture(CaptureWidth, CaptureHeight, 0),
            RenderTargetRight = new RenderTexture(CaptureWidth, CaptureHeight, 0),
            ResolveTexture = new Texture2D(CaptureWidth, CaptureHeight * 2),
            FrameCount = 0
        };

        CaptureFrame(
            frame,
            CaptureWidth,
            CaptureHeight,
            HorizontalAngularIncrement,
            VerticalAngularIncrement,
            EyeSeparation,
            EnableEffects,
            camera,
            transform.position,
            BiltMaterial);

        DumpFrame(
            frame,
            CaptureFolder,
            0,
            true);

        DestroyImmediate(frame.RenderTargetLeft);
        DestroyImmediate(frame.RenderTargetRight);
        DestroyImmediate(frame.ResolveTexture);
        DestroyImmediate(cameraRoot);
    }
}
