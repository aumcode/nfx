/*<FILE_LICENSE>
* NFX (.NET Framework Extension) Unistack Library
* Copyright 2003-2014 IT Adapter Inc / 2015 Aum Code LLC
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
</FILE_LICENSE>*/


using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using NFX.Geometry;

namespace NFX.WinForms.Controls
{
  
  [ToolboxItem(false)]
  public class Balloon : Form
  {
    #region CONSTS
      
      private const int BORDER_MARGIN = 24;
      private const int TEXT_BORDER_MARGIN = 4;
      
      private const double BEAT_AMPLITUDE = 0.0755d;
      
      private const double MIN_OPACITY = 0.01d;
      private const double MAX_OPACITY = 0.99d;
      
      private const int MAX_TEXT_PX_WIDTH = 1024;

      private const int DEF_BODY_LEFT = 100;
      private const int DEF_BODY_TOP = 100;
      private const int DEF_BODY_WIDTH = 250;
      private const int DEF_BODY_HEIGHT = 100;

      private const int DEF_TARGET_X = 350;
      private const int DEF_TARGET_Y = 320;
      
      private const int ANCHOR_TIMER_INTERVAL = 50;//ms
      private const int BEAT_TIMER_DEFAULT_INTERVAL = 25;//ms
      private const int FADE_TIMER_DEFAULT_INTERVAL = 25;//ms
    #endregion
     
     
      #region .ctor
       
        /// <summary>
        /// Creates new default balloon instance
        /// </summary>
        public Balloon() : base()
        {
           ctorBody(new Rectangle(DEF_BODY_LEFT, DEF_BODY_TOP,
                                  DEF_BODY_WIDTH, DEF_BODY_HEIGHT), 
                    new Point(DEF_TARGET_X, DEF_TARGET_Y), Color.Yellow);
        }
        
        /// <summary>
        /// Creates new balloon instance with specified coordinates
        /// </summary>
        public Balloon(Rectangle bodyRect, Point targetPoint, Color color) : base()
        {
           ctorBody(bodyRect, targetPoint, color);
        }

        /// <summary>
        /// Creates new instance of balloon infering its body size form supplied text
        /// </summary>
        /// <param name="bodyTopLeft">A position of balloon body</param>
        /// <param name="text">Text to display inside balloon</param>
        /// <param name="font">Balloon text font, may be null in which case default font is used</param>
        /// <param name="targetPoint">A point where ballon leg should point</param>
        /// <param name="color">Balloon color</param>
        public Balloon(Point bodyTopLeft, string text, Font font, Point targetPoint, Color color) : base()
        {
          base.Text = text ?? string.Empty;
          
          if (font!=null)
            Font = font;
          
          ctorBody(inferBodyRectFromText(bodyTopLeft, Text, Font), targetPoint, color);
        }
        
        /// <summary>
        /// Creates new isntance of balloon,infering its body size form supplied text and linking balloon leg with control,
        /// balloon will automatically follow control position on the screen even when it changes
        /// </summary>
        /// <param name="text">Text to display inside balloon</param>
        /// <param name="font">Balloon text font, may be null in which case default font is used</param>
        /// <param name="anchorControl">Control to anchor ballon leg to</param>
        /// <param name="legLength">Length of balloon leg</param>
        /// <param name="preferredPlacement">Where balloon should be placed when multiple placement positions are equaly available</param>
        /// <param name="color">Balloon color</param>
        public Balloon(string text, 
                       Font font, 
                       Control anchorControl,
                       int legLength,
                       MapDirection preferredPlacement, 
                       Color color) : base()
        {                  
          if (anchorControl==null)
            throw new WFormsException("Anchor controll can not be null"); //dont localize - very rare
          
          
          base.Text = text ?? string.Empty;
          if (font != null)
            Font = font;
            
           m_AnchorControl = anchorControl;
           m_LegLength = legLength; 
           m_PreferredPlacement = preferredPlacement;
         
          //need this to suppress dummy window when user does ALT+TAB
          Form owner = anchorControl.FindForm();
          if (owner!=null)
             this.Owner = owner;
            
          findPlacementCoordinates(out m_BodyRect, out m_TargetPoint);

          ctorBody(m_BodyRect, m_TargetPoint, color);

          m_AnchorTimer = new Timer();
          m_AnchorTimer.Interval = ANCHOR_TIMER_INTERVAL;
          m_AnchorTimer.Tick += new EventHandler(m_AnchorTimer_Tick);
          m_AnchorTimer.Enabled = true;
        }

        
        private void ctorBody(Rectangle bodyRect, Point targetPoint, Color color)
        {
          m_BodyRect = bodyRect;
          m_TargetPoint = targetPoint;
          m_BalloonColor = color;

            ControlBox = false;
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            AllowTransparency = true;
            MaximizeBox = false;
            MinimizeBox = false;
            Opacity = MAX_OPACITY;
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            TopMost = true;
            
          rebuild();
          
          m_BeatTimer = new Timer();
          m_BeatTimer.Interval = BEAT_TIMER_DEFAULT_INTERVAL;
          m_BeatTimer.Tick += new EventHandler(m_BeatTimer_Tick);

          m_FadeTimer = new Timer();
          m_FadeTimer.Interval = FADE_TIMER_DEFAULT_INTERVAL;
          m_FadeTimer.Tick += new EventHandler(m_FadeTimer_Tick);
        }

        protected override void Dispose(bool disposing)
        {
          m_BeatTimer.Enabled = false;
          m_BeatTimer.Dispose();

          m_FadeTimer.Enabled = false;
          m_FadeTimer.Dispose();
          
          if (m_AnchorTimer!=null)
          {
            m_AnchorTimer.Enabled = false;
            m_AnchorTimer.Dispose();
          }
          
          base.Dispose(disposing);
        }

       
      #endregion

      #region Private Fields
        private Rectangle m_BodyRect;
        private Rectangle m_CurrentBodyRect;
        private Point m_TargetPoint;
        private Color m_BalloonColor;
        private Timer m_BeatTimer;
        private Timer m_FadeTimer;
        private Timer m_AnchorTimer;
        private bool m_Beating;
        private bool m_DisposeOnFadeOut;
        private StringAlignment m_BalloonTextAlignment = StringAlignment.Near;
        private Control m_AnchorControl;
        private int m_LegLength;
        private MapDirection m_PreferredPlacement;
      #endregion

      #region Properties


        public override string Text
        {
          get
          {
            return base.Text;
          }
          set
          {
            if (base.Text!=value)
            {
              base.Text = value;
              
              if (m_AnchorControl!=null)
              {
                findPlacementCoordinates(out m_BodyRect, out m_TargetPoint);
                rebuild();
              }  
            }
          }
        }
        
        
        public new Color TransparencyKey 
        {
         get 
         {
          return base.TransparencyKey; 
         }  
        }
        
        public new Color BackColor
        {
         get
         {
           return base.BackColor;
         }
        }
        
        
        /// <summary>
        /// Screen coordinates of balloon main body rectangle
        /// </summary>
        public Rectangle BodyRect
        {
           get { return m_BodyRect; }
           set 
           {
              m_BodyRect = value;
              rebuild();    
           }
        }

        /// <summary>
        /// Screen point where balloon leg should point
        /// </summary>
        public Point TargetPoint
        {
          get { return m_TargetPoint; }
          set
          {
            m_TargetPoint = value;
            rebuild();
          }
        }

        /// <summary>
        /// Balloon color
        /// </summary>
        public Color BalloonColor
        {
          get { return m_BalloonColor; }
          set
          {
            m_BalloonColor = value;
            Invalidate();
          }
        }
        
        /// <summary>
        /// Indicates whether balloon beats like a heart
        /// </summary>
        public bool Beating
        {
          get { return m_Beating; }
          set 
          {
            if (value && !m_Beating)
            {
              m_BeatTimer.Enabled = true;
            }
            m_Beating = value;
          }
        }

        /// <summary>
        /// String alignment of text displayed in the balloon
        /// </summary>
        public StringAlignment BalloonTextAlignment
        {
          get { return m_BalloonTextAlignment; }
          set
          {
            m_BalloonTextAlignment = value;
            Refresh();
          }
        }
        
        /// <summary>
        /// When true, disposes balloon right after fade-out sequence finishes
        /// </summary>
        public bool DisposeOnFadeOut
        {
          get { return m_DisposeOnFadeOut;  }
          set { m_DisposeOnFadeOut = value; }
        }
        
      #endregion

      #region Public

        /// <summary>
        /// Gradually shows balloon
        /// </summary>
        public void FadeIn()
        {
          FadeIn(10, 15);
        }

        /// <summary>
        /// Gradually hides balloon
        /// </summary>
        public void FadeOut()
        {
          FadeOut(10, 15);
        }
        
        
        /// <summary>
        /// Gradually shows balloon
        /// </summary>
        /// <param name="interval">Timer interval</param>
        /// <param name="steps">Showing steps count</param>
        public void FadeIn(int interval, int steps)
        {
          Opacity = MIN_OPACITY;
         
          if (!Visible) 
             Visible = true;
         
          opacityDelta = (MAX_OPACITY - MIN_OPACITY) / steps;
          m_FadeTimer.Interval = interval;
          m_FadeTimer.Enabled = true;
        }
        
        /// <summary>
        /// Gradually hides balloon
        /// </summary>
        /// <param name="interval">Timer interval</param>
        /// <param name="steps">Hiding steps count</param>
        public void FadeOut(int interval, int steps)
        {
          Opacity = MAX_OPACITY;
          opacityDelta = - (MAX_OPACITY - MIN_OPACITY) / steps;
          m_FadeTimer.Interval = interval;
          m_FadeTimer.Enabled = true;
        }
        
        
        /// <summary>
        /// Erases binding to anchored control 
        /// </summary>
        public void DetachAnchoredControl()
        {
          m_AnchorControl = null;
          m_AnchorTimer.Stop();
        }
      


      #endregion

      #region Protected

        protected override void OnActivated(EventArgs e)
        {
          if (m_AnchorControl != null)
          {
            Form frm =  m_AnchorControl.FindForm();
            if (frm!=null)
               frm.Activate();
          }    
        }



        protected override void OnPaint(PaintEventArgs e)
        {
          //e.Graphics.DrawRectangle(Pens.Red, new Rectangle(1,1,Width-2,Height-2));
          
          Rectangle rect = m_CurrentBodyRect;
          rect.Offset(-Location.X, -Location.Y);
         
          Point trg = m_TargetPoint;
          trg.Offset(-Location.X, -Location.Y);

          //BaseApplication.Theme.PartRenderer.Balloon(e.Graphics, rect, trg, m_BalloonColor, null);

          //render Text

          double w = BEAT_AMPLITUDE * m_BodyRect.Width;
          
          rect = m_CurrentBodyRect;
          rect.Offset(-Location.X + TEXT_BORDER_MARGIN, -Location.Y + TEXT_BORDER_MARGIN);
          rect.Width = m_BodyRect.Width - (int)(w/2)  - TEXT_BORDER_MARGIN;
          rect.Height -= TEXT_BORDER_MARGIN*2;
        
          using(Brush txtBrush = new SolidBrush(this.ForeColor))
          {
           // BaseApplication.Theme.PartRenderer.Text(
           //                                      e.Graphics, 
           //                                      rect, 
           //                                      false,
           //                                      Font,
           //                                      txtBrush, 
           //                                      null, 
           //                                      Text,
           //                                      m_BalloonTextAlignment);
          }                                       
                     
          base.OnPaint(e);
        }


        protected override void OnTextChanged(EventArgs e)
        {
          Refresh();
          base.OnTextChanged(e);
        }

      #endregion

      #region Private Utils
        
              
        
        
                
        private void rebuild()
        {
          Invalidate();//Vista fix
          
           m_CurrentBodyRect = m_BodyRect;
        
           int left = m_BodyRect.Left < m_TargetPoint.X ? m_BodyRect.Left : m_TargetPoint.X;
           int top = m_BodyRect.Top < m_TargetPoint.Y ? m_BodyRect.Top : m_TargetPoint.Y;

           int right = m_BodyRect.Right > m_TargetPoint.X ? m_BodyRect.Right : m_TargetPoint.X;
           int bottom = m_BodyRect.Bottom > m_TargetPoint.Y ? m_BodyRect.Bottom : m_TargetPoint.Y;
           
           left-=BORDER_MARGIN; 
           top-=BORDER_MARGIN; 
           right+=BORDER_MARGIN;
           bottom+=BORDER_MARGIN;
           
           
           SetBounds(left, top, right - left, bottom - top);

           Color mask = Color.FromArgb(127, 127, 127);//VISTA BUG FIX, all components MUST be equal

           base.TransparencyKey = mask;
           base.BackColor = mask;
        }
        
        
        private int beatDirection = -1;
        
                
        private void m_BeatTimer_Tick(object sender, EventArgs e)
        {
          m_CurrentBodyRect.Inflate(beatDirection, beatDirection);
          
          double ratioW = (double)m_CurrentBodyRect.Width / (double)m_BodyRect.Width;
          double ratioH = (double)m_CurrentBodyRect.Height / (double)m_BodyRect.Height;
          
          double ratio = Math.Min(ratioW, ratioH);
          
          if ((ratio<(1-BEAT_AMPLITUDE))||(ratio>1d)) beatDirection = - beatDirection;
          
          if (!m_Beating && ratio>1d)
          {
            m_BeatTimer.Enabled = false;
          }
          else
          {
            int intrvl = 25 + (int)(40*((1-ratio)/BEAT_AMPLITUDE));
            if (intrvl<10) intrvl =10;
            m_BeatTimer.Interval = intrvl;
          }
          
          Refresh();
        }
        
        
        private double opacityDelta;
        private void m_FadeTimer_Tick(object sender, EventArgs e)
        {
          double op = Opacity + opacityDelta;
          if (op<MIN_OPACITY) op = MIN_OPACITY;
          if (op>MAX_OPACITY) op = MAX_OPACITY;
          Opacity = op;                       
                                           
          if ((op==MIN_OPACITY) || (op==MAX_OPACITY))
          {
             m_FadeTimer.Enabled = false;
            
             if (m_DisposeOnFadeOut && opacityDelta<0)
               this.Dispose();
          }
        }

        private Rectangle prior_ctl_Rect = new Rectangle(DEF_BODY_LEFT, DEF_BODY_TOP, DEF_BODY_WIDTH, DEF_BODY_HEIGHT);
        void m_AnchorTimer_Tick(object sender, EventArgs e)
        {
          if (m_AnchorControl==null) return;//safeguard
          
          bool topLevel = false;
          if ((m_AnchorControl.TopLevelControl!=null) && (m_AnchorControl.TopLevelControl is Form))
                 topLevel = (m_AnchorControl.TopLevelControl == Form.ActiveForm) || (Form.ActiveForm==this);
          
          Visible = m_AnchorControl.CanFocus && topLevel;
                                                                             
          Rectangle current = getAnchoredControlScreenRect();
          if (current!=prior_ctl_Rect)
          {
            prior_ctl_Rect = current;
            findPlacementCoordinates(out m_BodyRect, out m_TargetPoint);
            rebuild();
            
          }
        }




        private Rectangle inferBodyRectFromText(Point point, string text, Font font)
        {
          using(Graphics gr = CreateGraphics())
          {
            SizeF sz = gr.MeasureString(text, font, MAX_TEXT_PX_WIDTH);
            
            double wa = sz.Width * BEAT_AMPLITUDE;
            double ha = sz.Height * BEAT_AMPLITUDE;
            
            return new Rectangle(point.X,
                                 point.Y,
                                 (int)(sz.Width + wa) + TEXT_BORDER_MARGIN * 2,
                                 (int)(sz.Height + ha) + TEXT_BORDER_MARGIN * 2);
          }
        }


        private void findPlacementCoordinates(out Rectangle body, out Point target)
        {
            Point[] anchorPoints = calcAnchorPoints(1, 1);

            Rectangle brect = inferBodyRectFromText(new Point(0,0), Text, Font);
            Rectangle[] bodyRects = calcBodyRects(anchorPoints, brect.Size);

            body = bodyRects[(int)m_PreferredPlacement];
            target = anchorPoints[(int)m_PreferredPlacement];
            
            
            int minIdx = 0;
            int minArea = int.MaxValue;
            int area;
            
            for (int i = 0 ; i<8 ; i++)
            {
              Screen screen = Screen.FromRectangle(bodyRects[i]);
              
              area = CartesianUtils.CalculatePerimeterViolationArea(screen.Bounds, bodyRects[i]);
             
              if ( ((MapDirection)i == m_PreferredPlacement) && (area==0))
              {
                body = bodyRects[i];
                target = anchorPoints[i];
                return;
              }
              
              if (area<minArea)
              {
                minArea = area;
                minIdx = i;
              }
              
            }//for

            body = bodyRects[minIdx];
            target = anchorPoints[minIdx];
        }
        
        private Rectangle getAnchoredControlScreenRect()
        {
          if (m_AnchorControl==null) throw new WFormsException("AnchorControl==null");
          
          Point ctlLoc;
          if (m_AnchorControl.Parent != null)//safeguard
            ctlLoc = m_AnchorControl.Parent.PointToScreen(m_AnchorControl.Location);
          else
            ctlLoc = m_AnchorControl.PointToScreen(new Point(0, 0));

          return new Rectangle(ctlLoc, m_AnchorControl.Size);
        }
        
        
        private Point[] calcAnchorPoints(int dx, int dy)//takes leg-to-control distance
        {
             Point[] result = new Point[8];
             
                         
             Rectangle rect = getAnchoredControlScreenRect();
             
             result[(int)MapDirection.North] = new Point(rect.Left + (rect.Width / 2), rect.Top - dy);
             result[(int)MapDirection.South] = new Point(rect.Left + (rect.Width / 2), rect.Bottom + dy);
             result[(int)MapDirection.East] = new Point(rect.Right + dx, rect.Top + (rect.Height / 2));
             result[(int)MapDirection.West] = new Point(rect.Left - dx, rect.Top + (rect.Height / 2));
             
             result[(int)MapDirection.NorthEast] = new Point(rect.Right + dx, rect.Top - dy);
             result[(int)MapDirection.SouthEast] = new Point(rect.Right + dx, rect.Bottom + dy);
             result[(int)MapDirection.NorthWest] = new Point(rect.Left - dx, rect.Top - dy);
             result[(int)MapDirection.SouthWest] = new Point(rect.Left - dx, rect.Bottom + dy);
             
             return result;
        }

        private Rectangle[] calcBodyRects(Point[] anchorPoints, Size size)
        {
            Rectangle[] result = new Rectangle[8];

            Point anchor;
               
            anchor = anchorPoints[(int)MapDirection.North];
            result[(int)MapDirection.North] = new Rectangle(
                                                     anchor.X - size.Width / 2, 
                                                     anchor.Y - m_LegLength - size.Height,
                                                     size.Width,
                                                     size.Height);
                                                     
            anchor = anchorPoints[(int)MapDirection.South];
            result[(int)MapDirection.South] = new Rectangle(
                                                     anchor.X - size.Width / 2,
                                                     anchor.Y + m_LegLength,
                                                     size.Width,
                                                     size.Height);


            anchor = anchorPoints[(int)MapDirection.East];
            result[(int)MapDirection.East] = new Rectangle(
                                                     anchor.X + m_LegLength,
                                                     anchor.Y - size.Height / 2,
                                                     size.Width,
                                                     size.Height);

            anchor = anchorPoints[(int)MapDirection.West];
            result[(int)MapDirection.West] = new Rectangle(
                                                     anchor.X - m_LegLength - size.Width,
                                                     anchor.Y - size.Height / 2,
                                                     size.Width,
                                                     size.Height);


            anchor = anchorPoints[(int)MapDirection.NorthEast];
            result[(int)MapDirection.NorthEast] = new Rectangle(
                                                     anchor.X,
                                                     anchor.Y - m_LegLength - size.Height,
                                                     size.Width,
                                                     size.Height);

            anchor = anchorPoints[(int)MapDirection.SouthEast];
            result[(int)MapDirection.SouthEast] = new Rectangle(
                                                     anchor.X,
                                                     anchor.Y + m_LegLength,
                                                     size.Width,
                                                     size.Height);


            anchor = anchorPoints[(int)MapDirection.NorthWest];
            result[(int)MapDirection.NorthWest] = new Rectangle(
                                                     anchor.X - size.Width,
                                                     anchor.Y - m_LegLength - size.Height,
                                                     size.Width,
                                                     size.Height);

            anchor = anchorPoints[(int)MapDirection.SouthWest];
            result[(int)MapDirection.SouthWest] = new Rectangle(
                                                     anchor.X - size.Width,
                                                     anchor.Y + m_LegLength,
                                                     size.Width,
                                                     size.Height);

            return result;
        }
        
        
        
      #endregion




  }
}
