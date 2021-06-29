﻿using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.JSInterop;

namespace Havit.Blazor.Components.Web.Bootstrap
{
	/// <summary>
	/// Toast. Not intented to be used in user code, use <see cref="HxMessenger"/>.
	/// After first render component never updates.
	/// </summary>
	public partial class HxToast : ComponentBase, IAsyncDisposable
	{
		/// <summary>
		/// JS Runtime.
		/// </summary>
		[Inject] protected IJSRuntime JSRuntime { get; set; }

		/// <summary>
		/// Delay in miliseconds to automatically hide toast.
		/// </summary>
		[Parameter] public int? AutohideDelay { get; set; }

		/// <summary>
		/// Css class to render with toast.
		/// </summary>
		[Parameter] public string CssClass { get; set; }

		/// <summary>
		/// Header icon.
		/// </summary>
		[Parameter] public IconBase HeaderIcon { get; set; }

		/// <summary>
		/// Header text.
		/// </summary>
		[Parameter] public string HeaderText { get; set; }

		/// <summary>
		/// Header template.
		/// </summary>
		[Parameter] public RenderFragment HeaderTemplate { get; set; }

		/// <summary>
		/// Content (body) text.
		/// </summary>
		[Parameter] public string ContentText { get; set; }

		/// <summary>
		/// Content (body) template.
		/// </summary>
		[Parameter] public RenderFragment ContentTemplate { get; set; }

		/// <summary>
		/// Indicates whether to show close button.
		/// </summary>
		[Parameter] public bool ShowCloseButton { get; set; } = true;

		/// <summary>
		/// Fires when toast is hidden (button or autohide).
		/// </summary>
		[Parameter] public EventCallback OnToastHidden { get; set; }

		private ElementReference toastElement;
		private DotNetObjectReference<HxToast> dotnetObjectReference;
		private IJSObjectReference jsModule;

		public HxToast()
		{
			dotnetObjectReference = DotNetObjectReference.Create(this);
		}

		/// <inheritdoc />
		protected override void BuildRenderTree(RenderTreeBuilder builder)
		{
			builder.OpenRegion(0);
			base.BuildRenderTree(builder);
			builder.CloseRegion();

			bool renderHeader = !String.IsNullOrEmpty(HeaderText) || (HeaderTemplate != null) || (HeaderIcon != null);
			bool renderContent = !String.IsNullOrEmpty(ContentText) || (ContentTemplate != null) || (ShowCloseButton && !renderHeader);

			builder.OpenElement(100, "div");
			builder.AddAttribute(101, "role", "alert");
			builder.AddAttribute(102, "aria-live", "assertive");
			builder.AddAttribute(103, "aria-atomic", "true");
			builder.AddAttribute(104, "class", CssClassHelper.Combine("toast", CssClass));

			if (AutohideDelay != null)
			{
				builder.AddAttribute(110, "data-bs-delay", AutohideDelay);
			}
			else
			{
				builder.AddAttribute(111, "data-bs-autohide", "false");
			}
			builder.AddElementReferenceCapture(120, referenceCapture => toastElement = referenceCapture);

			if (renderHeader)
			{
				builder.OpenElement(200, "div");
				builder.AddAttribute(201, "class", "toast-header");

				if (HeaderIcon != null)
				{
					builder.OpenComponent(202, typeof(HxIcon));
					builder.AddAttribute(203, nameof(HxIcon.Icon), HeaderIcon);
					builder.AddAttribute(204, nameof(HxIcon.CssClass), "me-2");
					builder.CloseComponent();
				}

				if (!String.IsNullOrWhiteSpace(HeaderText))
				{
					builder.OpenElement(205, "strong");
					builder.AddAttribute(206, "class", "me-auto");
					builder.AddContent(207, HeaderText);
					builder.CloseElement(); // strong
				}
				builder.AddContent(208, HeaderTemplate);

				if (ShowCloseButton)
				{
					builder.OpenRegion(209);
					BuildRenderTree_CloseButton(builder, "ms-auto");
					builder.CloseRegion();
				}
				builder.CloseElement(); // toast-header				

			}

			if (renderContent)
			{
				builder.OpenElement(300, "div");
				builder.AddAttribute(301, "class", !renderHeader && ShowCloseButton ? "d-flex" : null);
				builder.OpenElement(302, "div");
				builder.AddAttribute(303, "class", "toast-body");
				builder.AddContent(304, ContentText);
				builder.AddContent(305, ContentTemplate);
				builder.CloseElement(); // toast-body

				if (!renderHeader && ShowCloseButton)
				{
					builder.OpenRegion(306);
					builder.OpenElement(307, "div");
					builder.AddAttribute(308, "class", "me-2 m-auto");
					BuildRenderTree_CloseButton(builder, null);
					builder.CloseElement();
					builder.CloseRegion();
				}

				builder.CloseElement();
			}

			builder.CloseElement(); // toast
		}

		private void BuildRenderTree_CloseButton(RenderTreeBuilder builder, string cssClass = null)
		{
			builder.OpenElement(100, "button");
			builder.AddAttribute(101, "type", "button");
			builder.AddAttribute(102, "class", CssClassHelper.Combine("btn-close", cssClass));
			builder.AddAttribute(103, "data-bs-dismiss", "toast");
			builder.CloseElement(); // button
		}

		/// <inheritdoc />
		protected override async Task OnAfterRenderAsync(bool firstRender)
		{
			await base.OnAfterRenderAsync(firstRender);

			if (firstRender)
			{
				// we need to manualy setup the toast.
				jsModule ??= await JSRuntime.InvokeAsync<IJSObjectReference>("import", "./_content/Havit.Blazor.Components.Web.Bootstrap/hxtoast.js");
				await jsModule.InvokeVoidAsync("show", toastElement, dotnetObjectReference);
			}
		}

		protected override bool ShouldRender()
		{
			// never update content to avoid collision with javascript
			return false;
		}

		/// <summary>
		/// Receive notification from javascript when message is hidden.
		/// </summary>
		[JSInvokable("HxToast_HandleToastHidden")]
		public async Task HandleToastHidden()
		{
			await OnToastHidden.InvokeAsync(null);
		}

		/// <inheritdoc />
		public async ValueTask DisposeAsync()
		{
			if (jsModule != null)
			{
				await jsModule.InvokeVoidAsync("dispose", toastElement);
				await jsModule.DisposeAsync();
			}

			dotnetObjectReference.Dispose();
		}
	}
}
