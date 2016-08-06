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
using System.Text;
using System.Runtime.InteropServices;
using System.Drawing;

namespace NFX.WinApi
{


  /// <summary>
  /// Provides managed wrappers to Windows Gdi.dll
  /// </summary>
  public static class GDIApi
  {
    private const string GDI32 = "GDI32.DLL";

    public struct Rect
    {
      public int Left, Top, Right, Bottom;
      public Rect(Rectangle r)
      {
        this.Left = r.Left;
        this.Top = r.Top;
        this.Bottom = r.Bottom;
        this.Right = r.Right;
      }
    }

    public enum TernaryRasterOperations : uint
    {
      SRCCOPY = 0x00CC0020,
      SRCPAINT = 0x00EE0086,
      SRCAND = 0x008800C6,
      SRCINVERT = 0x00660046,
      SRCERASE = 0x00440328,
      NOTSRCCOPY = 0x00330008,
      NOTSRCERASE = 0x001100A6,
      MERGECOPY = 0x00C000CA,
      MERGEPAINT = 0x00BB0226,
      PATCOPY = 0x00F00021,
      PATPAINT = 0x00FB0A09,
      PATINVERT = 0x005A0049,
      DSTINVERT = 0x00550009,
      BLACKNESS = 0x00000042,
      WHITENESS = 0x00FF0062
    }

    public struct LOGBRUSH
    {
      public BrushStyle lbStyle;        //brush style
      public UInt32 lbColor;            //colorref RGB(...)
      public HatchStyle lbHatch;        //hatch style
    }

    public enum BrushStyle : uint
    {
      BS_SOLID = 0,
      BS_HOLLOW = 1,
      BS_NULL = 1,
      BS_HATCHED = 2,
      BS_PATTERN = 3,
      BS_INDEXED = 4,
      BS_DIBPATTERN = 5,
      BS_DIBPATTERNPT = 6,
      BS_PATTERN8X8 = 7,
      BS_DIBPATTERN8X8 = 8,
      BS_MONOPATTERN = 9
    }

    public enum HatchStyle : int
    {
      HS_HORIZONTAL = 0,       /* ----- */
      HS_VERTICAL = 1,       /* ||||| */
      HS_FDIAGONAL = 2,       /* \\\\\ */
      HS_BDIAGONAL = 3,       /* ///// */
      HS_CROSS = 4,       /* +++++ */
      HS_DIAGCROSS = 5       /* xxxxx */
    }

    public enum FontWeight : int
    {
      FW_DONTCARE = 0,
      FW_THIN = 100,
      FW_EXTRALIGHT = 200,
      FW_LIGHT = 300,
      FW_NORMAL = 400,
      FW_MEDIUM = 500,
      FW_SEMIBOLD = 600,
      FW_BOLD = 700,
      FW_EXTRABOLD = 800,
      FW_HEAVY = 900,
    }

    public enum FontCharSet : byte
    {
      ANSI_CHARSET = 0,
      DEFAULT_CHARSET = 1,
      SYMBOL_CHARSET = 2,
      SHIFTJIS_CHARSET = 128,
      HANGEUL_CHARSET = 129,
      HANGUL_CHARSET = 129,
      GB2312_CHARSET = 134,
      CHINESEBIG5_CHARSET = 136,
      OEM_CHARSET = 255,
      JOHAB_CHARSET = 130,
      HEBREW_CHARSET = 177,
      ARABIC_CHARSET = 178,
      GREEK_CHARSET = 161,
      TURKISH_CHARSET = 162,
      VIETNAMESE_CHARSET = 163,
      THAI_CHARSET = 222,
      EASTEUROPE_CHARSET = 238,
      RUSSIAN_CHARSET = 204,
      MAC_CHARSET = 77,
      BALTIC_CHARSET = 186,
    }

    public enum FontPrecision : byte
    {
      OUT_DEFAULT_PRECIS = 0,
      OUT_STRING_PRECIS = 1,
      OUT_CHARACTER_PRECIS = 2,
      OUT_STROKE_PRECIS = 3,
      OUT_TT_PRECIS = 4,
      OUT_DEVICE_PRECIS = 5,
      OUT_RASTER_PRECIS = 6,
      OUT_TT_ONLY_PRECIS = 7,
      OUT_OUTLINE_PRECIS = 8,
      OUT_SCREEN_OUTLINE_PRECIS = 9,
      OUT_PS_ONLY_PRECIS = 10,
    }

    public enum FontClipPrecision : byte
    {
      CLIP_DEFAULT_PRECIS = 0,
      CLIP_CHARACTER_PRECIS = 1,
      CLIP_STROKE_PRECIS = 2,
      CLIP_MASK = 0xf,
      CLIP_LH_ANGLES = (1 << 4),
      CLIP_TT_ALWAYS = (2 << 4),
      CLIP_DFA_DISABLE = (4 << 4),
      CLIP_EMBEDDED = (8 << 4),
    }

    public enum FontQuality : byte
    {
      DEFAULT_QUALITY = 0,
      DRAFT_QUALITY = 1,
      PROOF_QUALITY = 2,
      NONANTIALIASED_QUALITY = 3,
      ANTIALIASED_QUALITY = 4,
      CLEARTYPE_QUALITY = 5,
      CLEARTYPE_NATURAL_QUALITY = 6,
    }

    [Flags]
    public enum FontPitchAndFamily : byte
    {
      DEFAULT_PITCH = 0,
      FIXED_PITCH = 1,
      VARIABLE_PITCH = 2,
      FF_DONTCARE = (0 << 4),
      FF_ROMAN = (1 << 4),
      FF_SWISS = (2 << 4),
      FF_MODERN = (3 << 4),
      FF_SCRIPT = (4 << 4),
      FF_DECORATIVE = (5 << 4),
    }

    public enum PenStyle
    {
      PS_SOLID = 0, //The pen is solid.
      PS_DASH = 1, //The pen is dashed.
      PS_DOT = 2, //The pen is dotted.
      PS_DASHDOT = 3, //The pen has alternating dashes and dots.
      PS_DASHDOTDOT = 4, //The pen has alternating dashes and double dots.
      PS_NULL = 5, //The pen is invisible.
      PS_INSIDEFRAME = 6
    };

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public class LOGFONT
    {
      public int lfHeight = 0;
      public int lfWidth = 0;
      public int lfEscapement = 0;
      public int lfOrientation = 0;
      public int lfWeight = 0;
      public byte lfItalic = 0;
      public byte lfUnderline = 0;
      public byte lfStrikeOut = 0;
      public byte lfCharSet = 0;
      public byte lfOutPrecision = 0;
      public byte lfClipPrecision = 0;
      public byte lfQuality = 0;
      public byte lfPitchAndFamily = 0;
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
      public string lfFaceName = string.Empty;
    }


    public enum StockObjects : int
    {
      WHITE_BRUSH = 0,
      LTGRAY_BRUSH = 1,
      GRAY_BRUSH = 2,
      DKGRAY_BRUSH = 3,
      BLACK_BRUSH = 4,
      NULL_BRUSH = 5,
      HOLLOW_BRUSH = NULL_BRUSH,
      WHITE_PEN = 6,
      BLACK_PEN = 7,
      NULL_PEN = 8,
      OEM_FIXED_FONT = 10,
      ANSI_FIXED_FONT = 11,
      ANSI_VAR_FONT = 12,
      SYSTEM_FONT = 13,
      DEVICE_DEFAULT_FONT = 14,
      DEFAULT_PALETTE = 15,
      SYSTEM_FIXED_FONT = 16,
      DEFAULT_GUI_FONT = 17,
      DC_BRUSH = 18,
      DC_PEN = 19,
    }

    public enum BinaryRasterOperations : int
    {
        R2_BLACK = 1,
        R2_NOTMERGEPEN = 2,
        R2_MASKNOTPEN = 3,
        R2_NOTCOPYPEN = 4,
        R2_MASKPENNOT = 5,
        R2_NOT = 6,
        R2_XORPEN = 7,
        R2_NOTMASKPEN = 8,
        R2_MASKPEN = 9,
        R2_NOTXORPEN = 10,
        R2_NOP = 11,
        R2_MERGENOTPEN = 12,
        R2_COPYPEN = 13,
        R2_MERGEPENNOT = 14,
        R2_MERGEPEN = 15,
        R2_WHITE = 16
    }

    public static UInt64 RGB(Int32 r, Int32 g, Int32 b)
    {
      return ((UInt64)(((Byte)(r) | ((UInt64)(g) << 8)) | (((UInt64)(Byte)(b)) << 16)));
    }


    [DllImport(GDI32)]
      public static extern IntPtr SelectObject(IntPtr hDc, IntPtr hGdiObj);

    [DllImport(GDI32)]
    public static extern Boolean DeleteObject(IntPtr handle);


    [DllImport(GDI32)]
    static extern IntPtr CreateCompatibleDC(IntPtr hDc);


    [DllImport(GDI32)]
    static extern bool DeleteDC(IntPtr hDc);

    [DllImport(GDI32)]
    static extern IntPtr CreateCompatibleBitmap(IntPtr hDc, Int32 nWidth, Int32 nHeight);


    [DllImport(GDI32)]
    public static extern Boolean BitBlt(IntPtr hObject, Int32 nXDest, Int32 nYDest, Int32 nWidth, Int32 nHeight, IntPtr hObjSource, Int32 nXSrc, Int32 nYSrc, TernaryRasterOperations dwRop);

    [DllImport(GDI32)]
      public static extern Boolean TextOut(IntPtr hDc, Int32 nXStart, Int32 nYStart,
          String lpString, Int32 cbString);

    [DllImport(GDI32)]
      public static extern Boolean MoveToEx(IntPtr hDc, Int32 x, Int32 y, IntPtr lpPoint);

    [DllImport(GDI32)]
      public static extern Boolean LineTo(IntPtr hDc, Int32 x, Int32 y);


    [DllImport(GDI32)]
    static extern Boolean Rectangle(IntPtr hDc, Int32 nLeftRect, Int32 nTopRect, Int32 nRightRect, Int32 nBottomRect);

    [DllImport(GDI32)]
    public static extern IntPtr CreateBrushIndirect([In] ref LOGBRUSH lplb);

    [DllImport(GDI32)]
    static extern IntPtr CreateFontIndirect([In] ref LOGFONT lplf);

    [DllImport(GDI32)]
    public static extern IntPtr CreatePen(int fnPenStyle, int nWidth, uint crColor);

    [DllImport(GDI32)]
    static extern IntPtr ExtCreatePen(UInt32 dwPenStyle, UInt32 dwWidth, ref LOGBRUSH lplb, UInt32 dwStyleCount, UInt32[] lpStyle);

    [DllImport(GDI32)]
    static extern IntPtr GetStockObject(Int32 fnObject);

    [DllImport(GDI32)]
    static extern UInt32 SetDCPenColor(IntPtr hDc, UInt32 crColor);

    [DllImport(GDI32)]
    static extern UInt32 SetDCBrushColor(IntPtr hDc, UInt32 crColor);

    [DllImport(GDI32)]
    static extern uint SetBkColor(IntPtr hDc, UInt32 crColor);


    const Int32 TRANSPARENT = 1;
    const Int32 OPAQUE = 2;

    [DllImport(GDI32)]
    static extern Int32 SetROP2(IntPtr hdc, BinaryRasterOperations ops);


    public const int
    TA_NOUPDATECP = 0,
    TA_UPDATECP = 1,

    TA_LEFT = 0,
    TA_RIGHT = 2,
    TA_CENTER = 6,

    TA_TOP = 0,
    TA_BOTTOM = 8,
    TA_BASELINE = 24,
    TA_RTLREADING = 256,
    TA_MASK = (TA_BASELINE + TA_CENTER + TA_UPDATECP + TA_RTLREADING);

    public const int
    VTA_BASELINE = TA_BASELINE,
    VTA_LEFT = TA_BOTTOM,
    VTA_RIGHT = TA_TOP,
    VTA_CENTER = TA_CENTER,
    VTA_BOTTOM = TA_RIGHT,
    VTA_TOP = TA_LEFT;

    [DllImport(GDI32)]
    static extern uint SetTextAlign(IntPtr hDc, UInt32 fMode);

    [DllImport(GDI32)]
    static extern UInt32 SetTextColor(IntPtr hDc, UInt32 crColor);

    [DllImport(GDI32)]
    static extern UInt32 GetTextColor(IntPtr hDc);

    [DllImport(GDI32)]
    static extern IntPtr CreateSolidBrush(UInt32 crColor);



  }


}