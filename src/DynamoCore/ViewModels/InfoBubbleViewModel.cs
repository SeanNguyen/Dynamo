﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Dynamo.Utilities;
using System.Windows.Threading;
using Dynamo.Controls;
using System.Collections.ObjectModel;
using Dynamo.Core;

namespace Dynamo.ViewModels
{
    public partial class InfoBubbleViewModel : ViewModelBase
    {
        public enum Style
        {
            LibraryItemPreview,
            NodeTooltip,
            Error,
            Preview,
            PreviewCondensed,
            None
        }
        public enum Direction
        {
            None,
            Left,
            Top,
            Right,
            Bottom,
            TopLeft,
            TopRight,
            BottomLeft,
            BottomRight
        }

        #region Properties

        private double zIndex = 3;
        public double ZIndex
        {
            get { return zIndex; }
            set { zIndex = value; RaisePropertyChanged("ZIndex"); }
        }
        private Style infoBubbleStyle = Style.None;
        public Style InfoBubbleStyle
        {
            get { return infoBubbleStyle; }
            set { infoBubbleStyle = value; RaisePropertyChanged("InfoBubbleStyle"); }
        }
        public string FullContent;
        public Direction ConnectingDirection = Direction.None;

        private bool isShowPreviewByDefault;
        public bool IsShowPreviewByDefault
        {
            get { return isShowPreviewByDefault; }
            set { isShowPreviewByDefault = value; RaisePropertyChanged("IsShowPreviewByDefault"); }
        }
        public double EstimatedWidth;
        public double EstimatedHeight;
        private PointCollection framePoints;
        public PointCollection FramePoints
        {
            get { return framePoints; }
            set { framePoints = value; RaisePropertyChanged("FramePoints"); }
        }
        private SolidColorBrush frameFill;
        public SolidColorBrush FrameFill
        {
            get
            {
                return frameFill;
            }
            set
            {
                frameFill = value; RaisePropertyChanged("FrameFill");
            }
        }
        private double frameStrokeThickness;
        public double FrameStrokeThickness
        {
            get { return frameStrokeThickness; }
            set { frameStrokeThickness = value; RaisePropertyChanged("FrameStrokeThickness"); }
        }
        private SolidColorBrush frameStrokeColor;
        public SolidColorBrush FrameStrokeColor
        {
            get { return frameStrokeColor; }
            set { frameStrokeColor = value; RaisePropertyChanged("FrameStrokeColor"); }
        }

        private Thickness margin;
        public Thickness Margin
        {
            get { return margin; }
            set { margin = value; RaisePropertyChanged("Margin"); }
        }
        private Thickness contentmargin;
        public Thickness ContentMargin
        {
            get { return contentmargin; }
            set { contentmargin = value; RaisePropertyChanged("ContentMargin"); }
        }
        private double maxWidth;
        public double MaxWidth
        {
            get { return maxWidth; }
            set { maxWidth = value; RaisePropertyChanged("MaxWidth"); }
        }
        private double maxHeight;
        public double MaxHeight
        {
            get { return maxHeight; }
            set { maxHeight = value; RaisePropertyChanged("MaxHeight"); }
        }
        private double minWidth;
        public double MinWidth
        {
            get { return minWidth; }
            set { minWidth = value; RaisePropertyChanged("MinWidth"); }
        }
        private double minHeight;
        public double MinHeight
        {
            get { return minHeight; }
            set { minHeight = value; RaisePropertyChanged("MinHeight"); }
        }

        private double contentMaxWidth;
        public double ContentMaxWidth
        {
            get { return contentMaxWidth; }
            set { contentMaxWidth = value; RaisePropertyChanged("ContentMaxWidth"); }
        }
        private double contentMaxHeight;
        public double ContentMaxHeight
        {
            get { return contentMaxHeight; }
            set { contentMaxHeight = value; RaisePropertyChanged("ContentMaxHeight"); }
        }

        private double opacity = 0;
        public double Opacity
        {
            get { return opacity; }
            set { opacity = value; RaisePropertyChanged("Opacity"); }
        }
        private Visibility infoBubbleVisibility;
        public Visibility InfoBubbleVisibility
        {
            get { return infoBubbleVisibility; }
            set { infoBubbleVisibility = value; RaisePropertyChanged("InfoBubbleVisibility"); }
        }

        private double textFontSize;
        public double TextFontSize
        {
            get { return textFontSize; }
            set { textFontSize = value; RaisePropertyChanged("TextFontSize"); }
        }
        private SolidColorBrush textForeground;
        public SolidColorBrush TextForeground
        {
            get { return textForeground; }
            set { textForeground = value; RaisePropertyChanged("TextForeground"); }
        }
        private FontWeight textFontWeight;
        public FontWeight TextFontWeight
        {
            get { return textFontWeight; }
            set { textFontWeight = value; RaisePropertyChanged("TextFontWeight"); }
        }
        private TextWrapping contentWrapping;
        public TextWrapping ContentWrapping
        {
            get { return contentWrapping; }
            set { contentWrapping = value; RaisePropertyChanged("TextWrapping"); }
        }
        private Visibility contentVisibility = Visibility.Visible;
        public Visibility ContentVisibility
        {
            get { return contentVisibility; }
            set { contentVisibility = value; RaisePropertyChanged("ContentVisibility"); }
        }
        private string content = string.Empty;
        public string Content
        {
            get { return content; }
            set { content = value; RaisePropertyChanged("Content"); }
        }

        public Point TargetTopLeft;
        public Point TargetBotRight;

        private Timer fadeInTimer;
        private Timer fadeOutTimer;
        private Direction limitedDirection = Direction.None;
        private bool alwaysVisible = false;

        private double preview_LastMaxWidth = double.MaxValue;
        private double preview_LastMaxHeight = double.MaxValue;

        public double Left
        {
            get { return 0; }
        }

        public double Top
        {
            get { return 0; }
        }

        #endregion

        #region Public Methods

        public InfoBubbleViewModel()
        {
            fadeInTimer = new Timer(20);
            fadeInTimer.Elapsed += fadeInTimer_Elapsed;

            fadeOutTimer = new Timer(20);
            fadeOutTimer.Elapsed += fadeOutTimer_Elapsed;
        }

        #endregion

        #region Command Methods

        private void UpdateInfoBubbleContent(object parameter)
        {
            InfoBubbleDataPacket data = (InfoBubbleDataPacket)parameter;

            UpdateStyle(data.Style, data.ConnectingDirection);
            UpdateContent(data.Text);
            UpdateShape(data.TopLeft, data.BotRight);
            UpdatePosition(data.TopLeft, data.BotRight);
        }

        private bool CanUpdateInfoBubbleCommand(object parameter)
        {
            return true;
        }

        private void UpdatePosition(object parameter)
        {
            InfoBubbleDataPacket data = (InfoBubbleDataPacket)parameter;
            SaveParameter(data.TopLeft, data.BotRight);
            UpdatePosition(data.TopLeft, data.BotRight);
        }

        private bool CanUpdatePosition(object parameter)
        {
            return true;
        }

        private void FadeIn(object parameter)
        {
            if (dynSettings.Controller.DynamoViewModel.IsMouseDown || 
                !dynSettings.Controller.DynamoViewModel.CurrentSpaceViewModel.CanShowInfoBubble)
                return;
            fadeOutTimer.Stop();
            fadeInTimer.Start();
        }

        private bool CanFadeIn(object parameter)
        {
            return true;
        }

        private void FadeOut(object parameter)
        {
            if (alwaysVisible)
                return;
            fadeInTimer.Stop();
            fadeOutTimer.Start();
        }

        private bool CanFadeOut(object parameter)
        {
            return true;
        }

        private void InstantCollapse(object parameter)
        {
            fadeInTimer.Stop();
            Opacity = 0;
        }

        private bool CanInstantCollapse(object parameter)
        {
            return true;
        }

        private void InstantAppear(object parameter)
        {
            Opacity = 0.95;
        }

        private bool CanInstantAppear(object parameter)
        {
            return true;
        }

        private void SetAlwaysVisible(object parameter)
        {
            alwaysVisible = (bool)parameter;
        }

        private bool CanSetAlwaysVisible(object parameter)
        {
            return true;
        }

        private void Resize(object parameter)
        {
            Point deltaPoint = (Point)parameter;

            double newMaxWidth = deltaPoint.X;
            double newMaxHeight = deltaPoint.Y;

            if (deltaPoint.X != double.MaxValue && newMaxWidth >= Configurations.PreviewMinWidth && newMaxWidth <= Configurations.PreviewMaxWidth)
            {
                MaxWidth = newMaxWidth;
                ContentMaxWidth = newMaxWidth - 10;
            }
            if (deltaPoint.Y != double.MaxValue && newMaxHeight >= Configurations.PreviewMinHeight)
            {
                MaxHeight = newMaxHeight;
                ContentMaxHeight = newMaxHeight - 17;
            }

            UpdateContent(FullContent);
            UpdateShape(TargetTopLeft, TargetBotRight);
            UpdatePosition(this.TargetTopLeft, this.TargetBotRight);

            this.preview_LastMaxWidth = MaxWidth;
            this.preview_LastMaxHeight = MaxHeight;
        }

        private bool CanResize(object parameter)
        {
            return true;
        }

        #endregion

        #region Private Helper Method

        private void UpdateContent(string text)
        {
            FullContent = text;
            if (this.infoBubbleStyle == Style.PreviewCondensed && text.Length > 25)
                Content = text.Substring(0, Configurations.CondensedPreviewMaxLength) + "...";
            else
                Content = text;
        }

        private void UpdatePosition(Point topLeft, Point botRight)
        {
            this.TargetTopLeft = topLeft;
            this.TargetBotRight = botRight;

            switch (InfoBubbleStyle)
            {
                case Style.LibraryItemPreview:
                    Margin = GetMargin_LibraryItemPreview(topLeft, botRight);
                    MakeFitInView();
                    break;
                case Style.NodeTooltip:
                    Margin = GetMargin_NodeTooltip(topLeft, botRight);
                    MakeFitInView();
                    break;
                case Style.Error:
                    Margin = GetMargin_Error(topLeft, botRight);
                    break;
                case Style.Preview:
                case Style.PreviewCondensed:
                    Margin = GetMargin_Preview(topLeft, botRight);
                    break;
            }
        }

        private void UpdateStyle(InfoBubbleViewModel.Style style, Direction connectingDirection)
        {
            if (infoBubbleStyle == style && this.ConnectingDirection == connectingDirection)
                return;
            InfoBubbleStyle = style;
            ConnectingDirection = connectingDirection;

            switch (style)
            {
                case Style.LibraryItemPreview:
                    SetStyle_LibraryItemPreview();
                    break;
                case Style.NodeTooltip:
                    SetStyle_NodeTooltip(connectingDirection);
                    break;
                case Style.Error:
                    SetStyle_Error();
                    break;
                case Style.Preview:
                    SetStyle_Preview();
                    break;
                case Style.PreviewCondensed:
                    SetStyle_PreviewCondensed();
                    break;
                case Style.None:
                    throw new ArgumentException("InfoWindow didn't have a style (456B24E0F400)");
            }
        }

        private void UpdateShape(Point topLeft, Point botRight)
        {
            switch (InfoBubbleStyle)
            {
                case Style.LibraryItemPreview:
                    FramePoints = GetFramePoints_LibraryItemPreview();
                    break;
                case Style.NodeTooltip:
                    FramePoints = GetFramePoints_NodeTooltip(topLeft, botRight);
                    break;
                case Style.Error:
                    FramePoints = GetFramePoints_Error();
                    break;
                case Style.Preview:
                case Style.PreviewCondensed:
                    FramePoints = GetFramePoints_Preview();
                    break;
                case Style.None:
                    break;
            }
        }

        private Thickness GetMargin_LibraryItemPreview(Point topLeft, Point botRight)
        {
            Thickness margin = new Thickness();
            margin.Top = (topLeft.Y + botRight.Y) / 2 - (EstimatedHeight / 2);
            return margin;
        }

        private Thickness GetMargin_NodeTooltip(Point topLeft, Point botRight)
        {
            Thickness margin = new Thickness();
            switch (ConnectingDirection)
            {
                case Direction.Bottom:
                    if (limitedDirection == Direction.TopRight)
                    {
                        margin.Top = botRight.Y - (botRight.Y - topLeft.Y) / 2;
                        margin.Left = topLeft.X - EstimatedWidth;
                    }
                    else if (limitedDirection == Direction.Top)
                    {
                        margin.Top = botRight.Y - (botRight.Y - topLeft.Y) / 2;
                        margin.Left = botRight.X;
                    }
                    else
                    {
                        margin.Top = topLeft.Y - EstimatedHeight;
                        margin.Left = (topLeft.X + botRight.X) / 2 - (EstimatedWidth / 2);
                    }
                    break;
                case Direction.Left:
                    if (limitedDirection == Direction.Right)
                    {
                        margin.Top = (topLeft.Y + botRight.Y) / 2 - (EstimatedHeight / 2);
                        margin.Left = topLeft.X - EstimatedWidth;
                    }
                    else
                    {
                        margin.Top = (topLeft.Y + botRight.Y) / 2 - (EstimatedHeight / 2);
                        margin.Left = botRight.X;
                    }
                    break;
                case Direction.Right:
                    if (limitedDirection == Direction.Left)
                    {
                        margin.Top = (topLeft.Y + botRight.Y) / 2 - (EstimatedHeight / 2);
                        margin.Left = botRight.X;
                    }
                    else
                    {
                        margin.Top = (topLeft.Y + botRight.Y) / 2 - (EstimatedHeight / 2);
                        margin.Left = topLeft.X - EstimatedWidth;
                    }
                    break;
            }
            return margin;
        }

        private Thickness GetMargin_Error(Point topLeft, Point botRight)
        {
            Thickness margin = new Thickness();
            double nodeWidth = botRight.X - topLeft.X;
            margin.Top = -(EstimatedHeight) + topLeft.Y;
            margin.Left = -((EstimatedWidth - nodeWidth) / 2) + topLeft.X;
            return margin;
        }

        private Thickness GetMargin_Preview(Point topLeft, Point botRight)
        {
            Thickness margin = new Thickness();
            double nodeWidth = botRight.X - topLeft.X;
            margin.Top = botRight.Y;
            margin.Left = -((EstimatedWidth - nodeWidth) / 2) + topLeft.X;

            if (!this.IsShowPreviewByDefault)
                margin.Top -= Configurations.PreviewArrowHeight;
            return margin;
        }

        private void SetStyle_LibraryItemPreview()
        {
            FrameFill = Configurations.LibraryTooltipFrameFill;
            FrameStrokeThickness = Configurations.LibraryTooltipFrameStrokeThickness;
            FrameStrokeColor = Configurations.LibraryTooltipFrameStrokeColor;

            MaxWidth = Configurations.LibraryTooltipMaxWidth;
            MaxHeight = Configurations.LibraryTooltipMaxHeight;
            ContentMaxWidth = Configurations.LibraryTooltipContentMaxWidth;
            ContentMaxHeight = Configurations.LibraryTooltipContentMaxHeight;

            TextFontSize = Configurations.LibraryTooltipTextFontSize;
            TextForeground = Configurations.LibraryTooltipTextForeground;
            TextFontWeight = Configurations.LibraryTooltipTextFontWeight;
            ContentWrapping = Configurations.LibraryTooltipContentWrapping;
            ContentMargin = Configurations.LibraryTooltipContentMargin;
        }

        private void SetStyle_NodeTooltip(Direction connectingDirection)
        {
            FrameFill = Configurations.NodeTooltipFrameFill;
            FrameStrokeThickness = Configurations.NodeTooltipFrameStrokeThickness;
            FrameStrokeColor = Configurations.NodeTooltipFrameStrokeColor;

            MaxWidth = Configurations.NodeTooltipMaxWidth;
            MaxHeight = Configurations.NodeTooltipMaxHeight;
            ContentMaxWidth = Configurations.NodeTooltipContentMaxWidth;
            ContentMaxHeight = Configurations.NodeTooltipContentMaxHeight;

            TextFontSize = Configurations.NodeTooltipTextFontSize;
            TextForeground = Configurations.NodeTooltipTextForeground;
            TextFontWeight = Configurations.NodeTooltipTextFontWeight;
            ContentWrapping = Configurations.NodeTooltipContentWrapping;

            switch (connectingDirection)
            {
                case Direction.Left:
                    ContentMargin = Configurations.NodeTooltipContentMarginLeft;
                    break;
                case Direction.Right:
                    ContentMargin = Configurations.NodeTooltipContentMarginRight;
                    break;
                case Direction.Bottom:
                    ContentMargin = Configurations.NodeTooltipContentMarginBottom;
                    break;
            }
        }

        private void SetStyle_Error()
        {
            FrameFill = Configurations.ErrorFrameFill;
            FrameStrokeThickness = Configurations.ErrorFrameStrokeThickness;
            FrameStrokeColor = Configurations.ErrorFrameStrokeColor;

            MaxWidth = Configurations.ErrorMaxWidth;
            MaxHeight = Configurations.ErrorMaxHeight;
            ContentMaxWidth = Configurations.ErrorContentMaxWidth;
            ContentMaxHeight = Configurations.ErrorContentMaxHeight;

            TextFontSize = Configurations.ErrorTextFontSize;
            TextForeground = Configurations.ErrorTextForeground;
            TextFontWeight = Configurations.ErrorTextFontWeight;
            ContentWrapping = Configurations.ErrorContentWrapping;
            ContentMargin = Configurations.ErrorContentMargin;
        }

        private void SetStyle_Preview()
        {
            FrameFill = Configurations.PreviewFrameFill;
            FrameStrokeThickness = Configurations.PreviewFrameStrokeThickness;
            FrameStrokeColor = Configurations.PreviewFrameStrokeColor;

            TextFontSize = Configurations.PreviewTextFontSize;
            TextForeground = Configurations.PreviewTextForeground;
            TextFontWeight = Configurations.PreviewTextFontWeight;
            ContentWrapping = Configurations.PreviewContentWrapping;
            ContentMargin = Configurations.PreviewContentMargin;

            if (this.preview_LastMaxHeight == double.MaxValue)
            {
                this.MaxHeight = Configurations.PreviewDefaultMaxHeight;
                this.ContentMaxHeight = Configurations.PreviewDefaultMaxHeight - 17;
            }
            else
            {
                this.MaxHeight = this.preview_LastMaxHeight;
                this.ContentMaxHeight = this.preview_LastMaxHeight - 17;
            }

            if (this.preview_LastMaxWidth == double.MaxValue)
            {
                this.MaxWidth = Configurations.PreviewDefaultMaxWidth;
                this.ContentMaxWidth = Configurations.PreviewDefaultMaxWidth - 10;
            }
            else
            {
                this.MaxWidth = this.preview_LastMaxWidth;
                this.ContentMaxWidth = this.preview_LastMaxWidth - 10;
            }

            MinHeight = Configurations.PreviewMinHeight;
            MinWidth = Configurations.PreviewMinWidth;
        }

        private void SetStyle_PreviewCondensed()
        {
            FrameFill = Configurations.PreviewFrameFill;
            FrameStrokeThickness = Configurations.PreviewFrameStrokeThickness;
            FrameStrokeColor = Configurations.PreviewFrameStrokeColor;

            MaxWidth = Configurations.PreviewCondensedMaxWidth;
            MinWidth = Configurations.PreviewCondensedMinWidth;
            MaxHeight = Configurations.PreviewCondensedMaxHeight;
            MinHeight = Configurations.PreviewCondensedMinHeight;
            ContentMaxWidth = Configurations.PreviewCondensedContentMaxWidth;
            ContentMaxHeight = Configurations.PreviewCondensedContentMaxHeight;

            TextFontSize = Configurations.PreviewTextFontSize;
            TextForeground = Configurations.PreviewTextForeground;
            TextFontWeight = Configurations.PreviewTextFontWeight;
            ContentWrapping = Configurations.PreviewContentWrapping;
            ContentMargin = Configurations.PreviewContentMargin;
        }

        private PointCollection GetFramePoints_LibraryItemPreview()
        {
            PointCollection pointCollection = new PointCollection();
            pointCollection.Add(new Point(EstimatedWidth, 0));
            pointCollection.Add(new Point(Configurations.LibraryTooltipArrowWidth, 0));
            pointCollection.Add(new Point(Configurations.LibraryTooltipArrowWidth, EstimatedHeight / 2 - Configurations.LibraryTooltipArrowHeight / 2));
            pointCollection.Add(new Point(0, EstimatedHeight / 2));
            pointCollection.Add(new Point(Configurations.LibraryTooltipArrowWidth, EstimatedHeight / 2 + Configurations.LibraryTooltipArrowHeight / 2));
            pointCollection.Add(new Point(Configurations.LibraryTooltipArrowWidth, EstimatedHeight));
            pointCollection.Add(new Point(EstimatedWidth, EstimatedHeight));

            return pointCollection;
        }

        private PointCollection GetFramePoints_NodeTooltip(Point topLeft, Point botRight)
        {
            switch (ConnectingDirection)
            {
                case Direction.Bottom:
                    return GetFramePoints_NodeTooltipConnectBottom(topLeft, botRight);
                case Direction.Left:
                    return GetFramePoints_NodeTooltipConnectLeft(topLeft, botRight);
                case Direction.Right:
                    return GetFramePoints_NodeTooltipConnectRight(topLeft, botRight);
            }
            return new PointCollection();
        }

        private PointCollection GetFramePoints_NodeTooltipConnectBottom(Point topLeft, Point botRight)
        {
            PointCollection pointCollection = new PointCollection();

            if (topLeft.Y - EstimatedHeight >= 40)
            {
                limitedDirection = Direction.None;
                pointCollection.Add(new Point(EstimatedWidth, 0));
                pointCollection.Add(new Point(0, 0));
                pointCollection.Add(new Point(0, EstimatedHeight - Configurations.NodeTooltipArrowHeight_BottomConnecting));
                pointCollection.Add(new Point((EstimatedWidth / 2) - Configurations.NodeTooltipArrowWidth_BottomConnecting / 2,
                    EstimatedHeight - Configurations.NodeTooltipArrowHeight_BottomConnecting));
                pointCollection.Add(new Point(EstimatedWidth / 2, EstimatedHeight));
                pointCollection.Add(new Point((EstimatedWidth / 2) + (Configurations.NodeTooltipArrowWidth_BottomConnecting / 2),
                    EstimatedHeight - Configurations.NodeTooltipArrowHeight_BottomConnecting));
                pointCollection.Add(new Point(EstimatedWidth, EstimatedHeight - Configurations.NodeTooltipArrowHeight_BottomConnecting));
            }
            else if (botRight.X + EstimatedWidth <= dynSettings.Controller.DynamoViewModel.WorkspaceActualWidth)
            {
                limitedDirection = Direction.Top;
                ContentMargin = Configurations.NodeTooltipContentMarginLeft;
                UpdateContent(Content);

                pointCollection.Add(new Point(EstimatedWidth, 0));
                pointCollection.Add(new Point(0, 0));
                pointCollection.Add(new Point(Configurations.NodeTooltipArrowWidth_SideConnecting, Configurations.NodeTooltipArrowHeight_SideConnecting / 2));
                pointCollection.Add(new Point(Configurations.NodeTooltipArrowWidth_SideConnecting, EstimatedHeight));
                pointCollection.Add(new Point(EstimatedWidth, EstimatedHeight));
            }
            else
            {
                limitedDirection = Direction.TopRight;
                ContentMargin = Configurations.NodeTooltipContentMarginRight;
                UpdateContent(Content);

                pointCollection.Add(new Point(EstimatedWidth, 0));
                pointCollection.Add(new Point(0, 0));
                pointCollection.Add(new Point(0, EstimatedHeight));
                pointCollection.Add(new Point(EstimatedWidth - Configurations.NodeTooltipArrowWidth_SideConnecting, EstimatedHeight));
                pointCollection.Add(new Point(EstimatedWidth - Configurations.NodeTooltipArrowWidth_SideConnecting, Configurations.NodeTooltipArrowHeight_SideConnecting / 2));

            }
            return pointCollection;
        }

        private PointCollection GetFramePoints_NodeTooltipConnectLeft(Point topLeft, Point botRight)
        {
            PointCollection pointCollection = new PointCollection();

            if (botRight.X + EstimatedWidth > dynSettings.Controller.DynamoViewModel.WorkspaceActualWidth)
            {
                limitedDirection = Direction.Right;
                ContentMargin = Configurations.NodeTooltipContentMarginRight;
                UpdateContent(Content);
                pointCollection = GetFramePoints_NodeTooltipConnectRight(topLeft, botRight);
            }
            else
            {
                pointCollection.Add(new Point(EstimatedWidth, 0));
                pointCollection.Add(new Point(Configurations.NodeTooltipArrowWidth_SideConnecting, 0));
                pointCollection.Add(new Point(Configurations.NodeTooltipArrowWidth_SideConnecting,
                    EstimatedHeight / 2 - Configurations.NodeTooltipArrowHeight_SideConnecting / 2));
                pointCollection.Add(new Point(0, EstimatedHeight / 2));
                pointCollection.Add(new Point(Configurations.NodeTooltipArrowWidth_SideConnecting,
                    EstimatedHeight / 2 + Configurations.NodeTooltipArrowHeight_SideConnecting / 2));
                pointCollection.Add(new Point(Configurations.NodeTooltipArrowWidth_SideConnecting, EstimatedHeight));
                pointCollection.Add(new Point(EstimatedWidth, EstimatedHeight));
            }
            return pointCollection;
        }

        private PointCollection GetFramePoints_NodeTooltipConnectRight(Point topLeft, Point botRight)
        {
            PointCollection pointCollection = new PointCollection();

            if (topLeft.X - EstimatedWidth < 0)
            {
                limitedDirection = Direction.Left;
                ContentMargin = Configurations.NodeTooltipContentMarginLeft;
                UpdateContent(Content);
                pointCollection = GetFramePoints_NodeTooltipConnectLeft(topLeft, botRight);
            }
            else
            {
                pointCollection.Add(new Point(EstimatedWidth - Configurations.NodeTooltipArrowWidth_SideConnecting, 0));
                pointCollection.Add(new Point(0, 0));
                pointCollection.Add(new Point(0, EstimatedHeight));
                pointCollection.Add(new Point(EstimatedWidth - Configurations.NodeTooltipArrowWidth_SideConnecting, EstimatedHeight));
                pointCollection.Add(new Point(EstimatedWidth - Configurations.NodeTooltipArrowWidth_SideConnecting,
                    EstimatedHeight / 2 + Configurations.NodeTooltipArrowHeight_SideConnecting / 2));
                pointCollection.Add(new Point(EstimatedWidth, EstimatedHeight / 2));
                pointCollection.Add(new Point(EstimatedWidth - Configurations.NodeTooltipArrowWidth_SideConnecting,
                    EstimatedHeight / 2 - Configurations.NodeTooltipArrowHeight_SideConnecting / 2));
            }
            return pointCollection;
        }

        private PointCollection GetFramePoints_Error()
        {
            PointCollection pointCollection = new PointCollection();
            pointCollection.Add(new Point(EstimatedWidth, 0));
            pointCollection.Add(new Point(0, 0));
            pointCollection.Add(new Point(0, EstimatedHeight - Configurations.ErrorArrowHeight));
            pointCollection.Add(new Point((EstimatedWidth / 2) - Configurations.ErrorArrowWidth / 2, EstimatedHeight - Configurations.ErrorArrowHeight));
            pointCollection.Add(new Point(EstimatedWidth / 2, EstimatedHeight));
            pointCollection.Add(new Point((EstimatedWidth / 2) + Configurations.ErrorArrowWidth / 2, EstimatedHeight - Configurations.ErrorArrowHeight));
            pointCollection.Add(new Point(EstimatedWidth, EstimatedHeight - Configurations.ErrorArrowHeight));
            return pointCollection;
        }

        private PointCollection GetFramePoints_Preview()
        {
            PointCollection pointCollection = new PointCollection();
            pointCollection.Add(new Point(EstimatedWidth, Configurations.PreviewArrowHeight));
            pointCollection.Add(new Point(EstimatedWidth / 2 + Configurations.PreviewArrowWidth / 2, Configurations.PreviewArrowHeight));
            pointCollection.Add(new Point(EstimatedWidth / 2, 0));
            pointCollection.Add(new Point(EstimatedWidth / 2 - Configurations.PreviewArrowWidth / 2, Configurations.PreviewArrowHeight));
            pointCollection.Add(new Point(0, Configurations.PreviewArrowHeight));
            pointCollection.Add(new Point(0, EstimatedHeight));
            pointCollection.Add(new Point(EstimatedWidth, EstimatedHeight));
            return pointCollection;
        }

        private void MakeFitInView()
        {
            //top
            if (Margin.Top <= 30)
            {
                Thickness newMargin = Margin;
                newMargin.Top = 40;
                Margin = newMargin;
            }
            //left
            if (Margin.Left <= 0)
            {
                Thickness newMargin = Margin;
                newMargin.Left = 0;
                this.Margin = newMargin;
            }
            //botton
            if (Margin.Top + EstimatedHeight >= dynSettings.Controller.DynamoViewModel.WorkspaceActualHeight)
            {
                Thickness newMargin = Margin;
                newMargin.Top = dynSettings.Controller.DynamoViewModel.WorkspaceActualHeight - EstimatedHeight - 1;
                Margin = newMargin;
            }
            //right
            if (Margin.Left + EstimatedWidth >= dynSettings.Controller.DynamoViewModel.WorkspaceActualWidth)
            {
                Thickness newMargin = Margin;
                newMargin.Left = dynSettings.Controller.DynamoViewModel.WorkspaceActualWidth - EstimatedWidth - 1;
                Margin = newMargin;
            }

        }

        private void SaveParameter(Point topLeft, Point botRight)
        {
            this.TargetTopLeft = topLeft;
            this.TargetBotRight = botRight;
        }

        private void fadeInTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (Opacity <= 0.95)
                Opacity += 0.95 / 10;
            else
                fadeInTimer.Stop();
        }

        private void fadeOutTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (Opacity >= 0)
                Opacity -= 0.85 / 10;
            else
                fadeOutTimer.Stop();
        }

        #endregion
    }

    public struct InfoBubbleDataPacket
    {
        public InfoBubbleViewModel.Style Style;
        public Point TopLeft;
        public Point BotRight;
        public string Text;
        public InfoBubbleViewModel.Direction ConnectingDirection;

        public InfoBubbleDataPacket(InfoBubbleViewModel.Style style, Point topLeft, Point botRight,
            string text, InfoBubbleViewModel.Direction connectingDirection)
        {
            Style = style;
            TopLeft = topLeft;
            BotRight = botRight;
            Text = text;
            ConnectingDirection = connectingDirection;
        }
    }
}
