using System;
using vke;
using Vulkan;

namespace VkvgPainter
{
	public class BaseWindow : CrowWindow
	{
		protected vkvg.Device vkvgDev;
		protected vkvg.Surface vkvgSurf;
		Image vkvgImage;
		protected override void initVulkan()
		{
			base.initVulkan();

			vkvgDev = new vkvg.Device (instance.Handle, phy.Handle, dev.VkDev.Handle, presentQueue.qFamIndex, Drawing2D.SampleCount.Sample_8);
		}

		public int CrowUpdateInterval {
			get => Crow.Interface.UPDATE_INTERVAL;
			set {
				if (Crow.Interface.UPDATE_INTERVAL == value)
					return;
				Crow.Interface.UPDATE_INTERVAL = value;
				NotifyValueChanged (Crow.Interface.UPDATE_INTERVAL);
			}
		}
		public long VkeUpdateInterval {
			get => UpdateFrequency;
			set {
				if (UpdateFrequency == value)
					return;
				UpdateFrequency = value;
				NotifyValueChanged (UpdateFrequency);
			}
		}
		DescriptorSet dsVkvgImg;
		protected override void CreateAndAllocateDescriptors()
		{
			descriptorPool = new DescriptorPool (dev, 2,
				new VkDescriptorPoolSize (VkDescriptorType.CombinedImageSampler, 2));
			descriptorSet = descriptorPool.Allocate (base.mainPipeline.Layout.DescriptorSetLayouts[0]);
			dsVkvgImg = descriptorPool.Allocate (base.mainPipeline.Layout.DescriptorSetLayouts[0]);
		}
		protected override void CreatePipeline()
		{
			using (GraphicPipelineConfig cfg = GraphicPipelineConfig.CreateDefault (VkPrimitiveTopology.TriangleList, VkSampleCountFlags.SampleCount1, false)) {
				cfg.Layout = new PipelineLayout (dev,
					new DescriptorSetLayout (dev,
						new VkDescriptorSetLayoutBinding (0, VkShaderStageFlags.Fragment, VkDescriptorType.CombinedImageSampler)
				));
				cfg.RenderPass = renderPass;

				cfg.blendAttachments [0] = new VkPipelineColorBlendAttachmentState (true);
				cfg.AddShader (dev, VkShaderStageFlags.Vertex, "#vke.FullScreenQuad.vert.spv");
				cfg.AddShader (dev, VkShaderStageFlags.Fragment, "#VkCrowWindow.simpletexture.frag.spv");

				mainPipeline = new GraphicPipeline (cfg);
			}
		}
		protected override void buildCommandBuffers()
		{
			dev.WaitIdle();

			vkvgSurf?.Dispose();
			vkvgSurf = new vkvg.Surface(vkvgDev, (int)Width, (int)Height);
			vkvgSurf.Clear();

			vkvgImage?.Dispose ();
			vkvgImage = new Image (dev, new VkImage ((ulong)vkvgSurf.VkImage.ToInt64 ()), VkFormat.B8g8r8a8Unorm,
				VkImageUsageFlags.Sampled, (uint)vkvgSurf.Width, (uint)vkvgSurf.Height);
			vkvgImage.CreateView ();
			vkvgImage.CreateSampler (VkFilter.Nearest, VkFilter.Nearest, VkSamplerMipmapMode.Nearest, VkSamplerAddressMode.ClampToBorder);
			vkvgImage.Descriptor.imageLayout = VkImageLayout.ShaderReadOnlyOptimal;

			DescriptorSetWrites dsUpdate = new DescriptorSetWrites (dsVkvgImg, base.mainPipeline.Layout.DescriptorSetLayouts[0]);
			dsUpdate.Write (dev, vkvgImage.Descriptor);

			dev.WaitIdle();

			base.buildCommandBuffers();
		}
		protected override void buildCommandBuffer (PrimaryCommandBuffer cmd, int imageIndex) {
			vkvgImage.SetLayout (cmd, VkImageAspectFlags.Color,
				VkImageLayout.ColorAttachmentOptimal, VkImageLayout.ShaderReadOnlyOptimal);

			mainPipeline.RenderPass.Begin (cmd, frameBuffers[imageIndex]);

			cmd.SetViewport (swapChain.Width, swapChain.Height);
			cmd.SetScissor (swapChain.Width, swapChain.Height);
			//common layout for both pipelines
			cmd.BindPipeline (mainPipeline);

			cmd.BindDescriptorSet (mainPipeline.Layout, dsVkvgImg);
			cmd.Draw (3, 1, 0, 0);
			cmd.BindDescriptorSet (mainPipeline.Layout, descriptorSet);
			cmd.Draw (3, 1, 0, 0);

			mainPipeline.RenderPass.End (cmd);

			vkvgImage.SetLayout (cmd, VkImageAspectFlags.Color,
				VkImageLayout.ShaderReadOnlyOptimal, VkImageLayout.ColorAttachmentOptimal);
		}
		protected override void render()
		{
			base.render();
			System.Threading.Thread.Sleep (1);
		}

		protected override void Dispose(bool disposing)
		{
			vkvgImage?.Dispose();

			vkvgSurf.Dispose();
			vkvgDev.Dispose();

			base.Dispose(disposing);
		}
	}
}
