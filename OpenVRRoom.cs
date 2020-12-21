using System;
using System.Collections.Generic;
using System.Threading;
using Valve.VR;

namespace RpgVR
{
	internal static class OpenVRRoom
	{
		internal static Thread Thread;
		internal static bool Closing;
		internal static CVRSystem System;
		internal static CVRCompositor Compositor;
		internal static TrackedDevicePose_t[] CurrentPoses;
		internal static TrackedDevicePose_t[] NextPoses;
		internal static List<uint> Controllers;
		private static RenderModel_t[] ControllerModels;
		private static RenderModel_TextureMap_t[] ControllerTextures;
		private static ShaderResourceView[] ControllerTextureViews;
		private static SharpDX.Direct3D11.Buffer[] ControllerVertexBuffers;
		private static SharpDX.Direct3D11.Buffer[] ControllerIndexBuffers;
		private static VertexBufferBinding[] ControllerVertexBufferBindings;
		private static Emitter[] ControllerEmitters;
		private static SourceVoice[] ControllerVoices;
		private static uint Headset;

		internal static void Enable()
		{
			var initError = EVRInitError.None;

			System = OpenVR.Init(ref initError);

			if (initError != EVRInitError.None)
				return;

			Compositor = OpenVR.Compositor;

			Compositor.CompositorBringToFront();
			Compositor.FadeGrid(5.0f, false);

			var count = OpenVR.k_unMaxTrackedDeviceCount;

			CurrentPoses = new TrackedDevicePose_t[count];
			NextPoses = new TrackedDevicePose_t[count];

			Controllers = new List<uint>();
			ControllerModels = new RenderModel_t[count];
			ControllerTextures = new RenderModel_TextureMap_t[count];
			ControllerTextureViews = new ShaderResourceView[count];
			ControllerVertexBuffers = new SharpDX.Direct3D11.Buffer[count];
			ControllerIndexBuffers = new SharpDX.Direct3D11.Buffer[count];
			ControllerVertexBufferBindings = new VertexBufferBinding[count];
			ControllerEmitters = new Emitter[count];
			ControllerVoices = new SourceVoice[count];

			for (uint device = 0; device < count; device++)
			{
				var deviceClass = System.GetTrackedDeviceClass(device);

				switch (deviceClass)
				{
					case ETrackedDeviceClass.HMD:
						Headset = device;
						break;

					case ETrackedDeviceClass.Controller:
						Controllers.Add(device);
						break;
				}
			}

			uint width = 0;
			uint height = 0;

			System.GetRecommendedRenderTargetSize(ref width, ref height);

			headsetSize = new Size((int)width, (int)height);
			windowSize = new Size(960, 540);

			var leftEyeProjection = Convert(System.GetProjectionMatrix(EVREye.Eye_Left, 0.01f, 1000.0f));
			var rightEyeProjection = Convert(System.GetProjectionMatrix(EVREye.Eye_Right, 0.01f, 1000.0f));

			var leftEyeView = Convert(System.GetEyeToHeadTransform(EVREye.Eye_Left));
			var rightEyeView = Convert(System.GetEyeToHeadTransform(EVREye.Eye_Right));

			foreach (var controller in Controllers)
			{
				var modelName = new StringBuilder(255, 255);
				var propertyError = ETrackedPropertyError.TrackedProp_Success;

				var length = System.GetStringTrackedDeviceProperty(controller, ETrackedDeviceProperty.Prop_RenderModelName_String, modelName, 255, ref propertyError);

				if (propertyError == ETrackedPropertyError.TrackedProp_Success)
				{
					var modelName2 = modelName.ToString();

					while (true)
					{
						var pointer = IntPtr.Zero;
						var modelError = EVRRenderModelError.None;

						modelError = OpenVR.RenderModels.LoadRenderModel_Async(modelName2, ref pointer);

						if (modelError == EVRRenderModelError.Loading)
							continue;

						if (modelError == EVRRenderModelError.None)
						{
							var renderModel = global::System.Runtime.InteropServices.Marshal.PtrToStructure<RenderModel_t>(pointer);

							ControllerModels[controller] = renderModel;
							break;
						}
					}

					while (true)
					{
						var pointer = IntPtr.Zero;
						var textureError = EVRRenderModelError.None;

						textureError = OpenVR.RenderModels.LoadTexture_Async(ControllerModels[controller].diffuseTextureId, ref pointer);

						if (textureError == EVRRenderModelError.Loading)
							continue;

						if (textureError == EVRRenderModelError.None)
						{
							var texture = global::System.Runtime.InteropServices.Marshal.PtrToStructure<RenderModel_TextureMap_t>(pointer);

							ControllerTextures[controller] = texture;
							break;
						}
					}
				}
			}

			int adapterIndex = 0;

			System.GetDXGIOutputInfo(ref adapterIndex);

			Thread = new Thread(Run);
			Thread.Start();
		}

		private static void Run(object parameter)
		{
			
		}

		internal static void Disable()
		{
			throw new NotImplementedException();
		}
	}
}