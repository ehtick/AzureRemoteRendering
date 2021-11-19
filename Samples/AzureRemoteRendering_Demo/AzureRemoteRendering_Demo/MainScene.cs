using System;
using System.Diagnostics;
using WaveEngine.AzureRemoteRendering;
using WaveEngine.AzureRemoteRendering.Components;
using WaveEngine.Components.WorkActions;
using WaveEngine.Framework;
using WaveEngine.MRTK.Scenes;

namespace AzureRemoteRendering_Demo
{
    public class MainScene : XRScene
    {
        protected override Guid CursorMatPressed => WaveContent.MRTK.Materials.CursorLeftPinch;

        protected override Guid CursorMatReleased => WaveContent.MRTK.Materials.CursorLeft;

        protected override Guid HoloHandsMat => Guid.Empty;

        protected override Guid HolographicEffect => WaveContent.MRTK.Effects.HoloGraphic;

        protected override Guid SpatialMappingMat => Guid.Empty;

        protected override Guid HandRayTexture => WaveContent.MRTK.Textures.line_dots_png;

        protected override Guid HandRaySampler => WaveContent.MRTK.Samplers.LinearWrapSampler;

        protected override void CreateScene()
        {
            base.CreateScene();

            var remoteSessionConfig = this.Managers.EntityManager.FindFirstComponentOfType<ARRSessionManager>().AccountInfo;

            // Users need to fill out the following with their account data and model
            remoteSessionConfig.AccountId = "00000000-0000-0000-0000-000000000000";
            remoteSessionConfig.AccountKey = "<account key>";
            remoteSessionConfig.AccountDomain = "westeurope.mixedreality.azure.com"; // <change to your region>
            remoteSessionConfig.RemoteRenderingDomain = "westeurope.mixedreality.azure.com"; // <change to your region>

            var modelLoader = this.Managers.EntityManager.FindFirstComponentOfType<ARRModelLoader>();
            modelLoader.Url = "builtin://Engine";

            var skyboxUrl = "builtin://IndustrialPipeAndValve";
            var arrService = Application.Current.Container.Resolve<AzureRemoteRenderingService>();
            this.CreateWaitConditionWorkAction(() => arrService.IsCurrentSessionConnected)
                .ContinueWithAction(async () =>
                {
                    var inspectorPath = await arrService.ConnectToArrInspectorAsync();
                    Debug.WriteLine($"[ARR] ArrInspector at: {inspectorPath}");

                    if (!string.IsNullOrEmpty(skyboxUrl))
                    {
                        Debug.WriteLine($"[ARR] Loading skybox texture \"{skyboxUrl}\"");
                        var skyboxTexture = await arrService.LoadTextureFromSASAsync(skyboxUrl, Microsoft.Azure.RemoteRendering.TextureType.CubeMap);
                        arrService.CurrentSession.Connection.SkyReflectionSettings.SkyReflectionTexture = skyboxTexture;
                    }
                })
                .Run();
        }
    }
}