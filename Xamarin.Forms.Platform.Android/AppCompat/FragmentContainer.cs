using System;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.App;
using Android.Views;
using AView = Android.Views.View;

namespace Xamarin.Forms.Platform.Android.AppCompat
{
	internal class FragmentContainer : Fragment
	{
		readonly WeakReference _pageReference;

		bool? _isVisible;
		PageContainer _pageContainer;
		IVisualElementRenderer _visualElementRenderer;

		public FragmentContainer()
		{
		}

		public FragmentContainer(Page page) : this()
		{
			_pageReference = new WeakReference(page);
		}

		protected FragmentContainer(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
		{
		}

		public Page Page => (Page)_pageReference?.Target;

		public override bool UserVisibleHint
		{
			get { return base.UserVisibleHint; }
			set
			{
				base.UserVisibleHint = value;
				if (_isVisible == value)
					return;
				_isVisible = value;
				if (_isVisible.Value)
					((IPageController)Page)?.SendAppearing();
				else
					((IPageController)Page)?.SendDisappearing();
			}
		}

		public static Fragment CreateInstance(Page page)
		{
			return new FragmentContainer(page) { Arguments = new Bundle() };
		}

		public override AView OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			if (Page != null)
			{
				_visualElementRenderer = Android.Platform.CreateRenderer(Page, ChildFragmentManager);
				Android.Platform.SetRenderer(Page, _visualElementRenderer);

				_pageContainer = new PageContainer(Forms.Context, _visualElementRenderer, true);
				return _pageContainer;
			}

			return null;
		}

		public override void OnDestroyView()
		{
			if (Page != null)
			{
				IVisualElementRenderer renderer = _visualElementRenderer;
				PageContainer container = _pageContainer;

				if (container.Handle != IntPtr.Zero && renderer.ViewGroup.Handle != IntPtr.Zero)
				{
					container.RemoveFromParent();
					renderer.ViewGroup.RemoveFromParent();
					Page.ClearValue(Android.Platform.RendererProperty);

					container.Dispose();
					renderer.Dispose();
				}
			}

			_visualElementRenderer = null;
			_pageContainer = null;

			base.OnDestroyView();
		}

		public override void OnHiddenChanged(bool hidden)
		{
			base.OnHiddenChanged(hidden);

			if (Page == null)
				return;

			if (hidden)
				((IPageController)Page).SendDisappearing();
			else
				((IPageController)Page).SendAppearing();
		}

		public override void OnPause()
		{
			((IPageController)Page)?.SendDisappearing();
			base.OnPause();
		}
		
		public override void OnResume()
		{
			((IPageController)Page)?.SendAppearing();
			base.OnResume();
		}
	}
}