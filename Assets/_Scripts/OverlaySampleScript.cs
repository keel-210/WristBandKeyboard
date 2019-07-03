/**
 * OpenVR Overlay samlpe by gpsnmeajp v0.2
 * 2018/08/25
 * 
 * v0.1 公開
 * v0.2 エラーチェックが不完全だった問題を修正。RenderTextureが無効なままセットしていた問題を修正
 * 
 * 2DのテクスチャをVR空間にオーバーレイ表示します。
 * 現在動作中のアプリケーションに関係なくオーバーレイすることができます。
 * 
 * 入力機能は正常に動作していないようなので省いています。
 * ダッシュボードオーバーレイは省略しています。
 * 
 * 各メソッドの詳細はValveSoftwareのIVROverlay_Overviewを確認してください。
 * https://github.com/ValveSoftware/openvr/wiki/IVROverlay_Overview
 *
 * These codes are licensed under CC0.
 * http://creativecommons.org/publicdomain/zero/1.0/deed.ja
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR; //Steam VR

public class OverlaySampleScript : MonoBehaviour
{
    //エラーメッセージの名前
    const string Tag = "[OverlaySample]";

    //グローバルキー(システムのオーバーレイ同士の識別名)。
    //ユニークでなければならない。乱数やUUIDなどを勧める
    const string OverlayKey = "[OverlaySample]";

    //ユーザーが確認するためのオーバーレイの名前
    const string OverlayFriendlyName = "OverlaySampleApplication";

    //オーバーレイのハンドル(整数)
    ulong overlayHandle = 0;

    //OpenVRシステムインスタンス
    public CVRSystem openvr;

    //Overlayインスタンス
    CVROverlay overlay;

    //オーバーレイに渡すネイティブテクスチャ
    Texture_t overlayTexture;

    //上下反転フラグ
    int textureYflip = 1;

    //HMD視点位置変換行列
    HmdMatrix34_t pose;

    //取得元のRenderTexture
    public RenderTexture renderTexture;
    public Transform RenderTarget;

    void Start()
    {
        var openVRError = EVRInitError.None;
        var overlayError = EVROverlayError.None;

        //OpenVRの初期化
        openvr = OpenVR.Init(ref openVRError, EVRApplicationType.VRApplication_Overlay);
        if (openVRError != EVRInitError.None)
        {
            Debug.LogError(Tag + "OpenVRの初期化に失敗." + openVRError.ToString());
            ApplicationQuit();
            return;
        }

        //オーバーレイ機能の初期化
        overlay = OpenVR.Overlay;
        overlayError = overlay.CreateOverlay(OverlayKey, OverlayFriendlyName, ref overlayHandle);
        if (overlayError != EVROverlayError.None)
        {
            Debug.LogError(Tag + "Overlayの初期化に失敗. " + overlayError.ToString());
            ApplicationQuit();
            return;
        }

        //オーバーレイの大きさ設定(幅のみ。高さはテクスチャの比から自動計算される)
        var width = 2.0f;
        overlay.SetOverlayWidthInMeters(overlayHandle, width);
        //オーバーレイの透明度を設定
        var alpha = 0.5f;
        overlay.SetOverlayAlpha(overlayHandle, alpha);

        //オーバーレイに渡すテクスチャ種類の設定
        var isOpenGL = SystemInfo.graphicsDeviceVersion.Contains("OpenGL");
        if (isOpenGL)
        {
            //pGLuintTexture
            overlayTexture.eType = ETextureType.OpenGL;
            //上下反転しない
            textureYflip = 1;
        }
        else
        {
            //pTexture
            overlayTexture.eType = ETextureType.DirectX;
            //上下反転する
            textureYflip = -1;
        }
        Debug.Log(Tag + "初期化完了しました");
    }

    void Update()
    {
        //初期化失敗するなどoverlayが無効な場合は実行しない
        if (overlay == null)
        {
            return;
        }

        //オーバーレイを表示する
        overlay.ShowOverlay(overlayHandle);

        //オーバーレイを非表示にする
        //overlay.HideOverlay(overlayHandle);

        //オーバーレイが表示されている時
        if (overlay.IsOverlayVisible(overlayHandle))
        {

            var vrcam = SteamVR_Render.Top();
            var offset = new SteamVR_Utils.RigidTransform(vrcam.origin, RenderTarget);
            if (RenderTarget != null)
            {
                offset.pos = RenderTarget.transform.position;
                offset.rot = RenderTarget.transform.rotation;
            }
            else
            {
                offset.pos = new Vector3(0, 0, 0);
                offset.rot = Quaternion.Euler(0, 0, 0);
            }
            var t = offset.ToHmdMatrix34();
            overlay.SetOverlayTransformAbsolute(overlayHandle, SteamVR_Render.instance.trackingSpace, ref t);

            //RenderTextureが生成されているかチェック
            if (!renderTexture.IsCreated())
            {
                Debug.Log(Tag + "RenderTextureがまだ生成されていない");
                return;
            }

            //RenderTextureからネイティブテクスチャのハンドルを取得
            try
            {
                overlayTexture.handle = renderTexture.GetNativeTexturePtr();
            }
            catch (UnassignedReferenceException e)
            {
                Debug.LogError(Tag + "RenderTextureがセットされていません");
                ApplicationQuit();
                return;
            }

            //オーバーレイにテクスチャを設定
            var overlayError = EVROverlayError.None;
            overlayError = overlay.SetOverlayTexture(overlayHandle, ref overlayTexture);
            if (overlayError != EVROverlayError.None)
            {
                Debug.LogError(Tag + "Overlayにテクスチャをセットできませんでした. " + overlayError.ToString());
                ApplicationQuit();
                return;
            }
        }
    }

    void OnApplicationQuit()
    {
        //アプリケーション終了時にOverlayハンドルを破棄する
        if (overlay != null)
        {
            overlay.DestroyOverlay(overlayHandle);
        }
        //VRシステムをシャットダウンする
        OpenVR.Shutdown();
    }

    //アプリケーションを終了させる
    void ApplicationQuit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}