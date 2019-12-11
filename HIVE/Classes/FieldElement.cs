using System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Shapes;

namespace HIVE
{
    public enum Marktypes
    {
        VOID,
        ACCESSIBLE,
        INACCESSIBLE,
        SELECTED
    }

    public enum ElementType
    {
        VOID, 
        WHITE_QUEEN,   BLACK_QUEEN,
        WHITE_ANT,     BLACK_ANT,
        WHITE_HOPPER,  BLACK_HOPPER,
        WHITE_SPIDER,  BLACK_SPIDER,
        WHITE_BEETLES, BLACK_BEETLES
    }

    public class Element
    {
        private readonly SolidColorBrush baseColor = new SolidColorBrush(Windows.UI.Colors.LightSteelBlue);
        private readonly SolidColorBrush baseBorderColor = new SolidColorBrush(Windows.UI.Colors.SteelBlue);

        private readonly SolidColorBrush accessibleColor = new SolidColorBrush(Windows.UI.Colors.Lime);
        private readonly SolidColorBrush accessibleBorderColor = new SolidColorBrush(Windows.UI.Colors.LimeGreen);

        private readonly SolidColorBrush inaccessibleColor = new SolidColorBrush(Windows.UI.Colors.Tomato);
        private readonly SolidColorBrush inaccessibleBorderColor = new SolidColorBrush(Windows.UI.Colors.OrangeRed);

        private readonly SolidColorBrush selectedBorderColor = new SolidColorBrush(Windows.UI.Colors.Gold);

        private ElementType type;
        private ElementType lastType;
        private Marktypes markType;

        private Polygon polygon;
        private PointCollection polygonPoints;

        private Polyline border;
        private Polyline markedBorder = null;
        private PointCollection borderPoints;

        private Action<Element> tappCallback;

        private dynamic GameBoardGrid;

        public Element(Action<Element> tappCallback, ElementType type, dynamic GameBoardGrid, double X, double Y, double width, double height)
        {
            this.GameBoardGrid = GameBoardGrid;

            this.type = type;
            this.tappCallback = tappCallback;

            this.polygon = new Polygon();
            polygon.Tapped += ShapeTapped;

            this.border = new Polyline();
            this.border.Stroke = baseBorderColor;
            this.border.StrokeThickness = 4;

            //     2---3
            //    /     \
            //  1,7      4
            //    \     /
            //     6---5
            dynamic p1 = new { X = X, Y = Y + height / 2 };
            dynamic p2 = new { X = X + width / 4, Y = Y };
            dynamic p3 = new { X = X + width / 4 * 3, Y = Y };
            dynamic p4 = new { X = X + width, Y = Y + height / 2 };
            dynamic p5 = new { X = X + width / 4 * 3, Y = Y + height };
            dynamic p6 = new { X = X + width / 4, Y = Y + height };

            polygonPoints = new PointCollection();
            polygonPoints.Add(new Windows.Foundation.Point(p1.X, p1.Y)); //1
            polygonPoints.Add(new Windows.Foundation.Point(p2.X, p2.Y)); //2
            polygonPoints.Add(new Windows.Foundation.Point(p3.X, p3.Y)); //3
            polygonPoints.Add(new Windows.Foundation.Point(p4.X, p4.Y)); //4
            polygonPoints.Add(new Windows.Foundation.Point(p5.X, p5.Y)); //5
            polygonPoints.Add(new Windows.Foundation.Point(p6.X, p6.Y)); //6

            this.polygon.Points = polygonPoints;

            borderPoints = new PointCollection();
            borderPoints.Add(new Windows.Foundation.Point(p1.X, p1.Y)); //1
            borderPoints.Add(new Windows.Foundation.Point(p2.X, p2.Y)); //2
            borderPoints.Add(new Windows.Foundation.Point(p3.X, p3.Y)); //3
            borderPoints.Add(new Windows.Foundation.Point(p4.X, p4.Y)); //4
            borderPoints.Add(new Windows.Foundation.Point(p5.X, p5.Y)); //5
            borderPoints.Add(new Windows.Foundation.Point(p6.X, p6.Y)); //6
            borderPoints.Add(new Windows.Foundation.Point(p1.X, p1.Y)); //7
            this.border.Points = borderPoints;

            GameBoardGrid.Children.Add(this.polygon);
            GameBoardGrid.Children.Add(this.border);

            this.MarkType = Marktypes.VOID;
        }

        private void ShapeTapped(object sender, TappedRoutedEventArgs e)
        {
            this.tappCallback(this);
        }

        public Polygon Polygon
        {
            get
            {
                return this.polygon;
            }
        }

        public ElementType Type
        {
            get
            {
                return type;
            }
            set
            {
                if (value == ElementType.WHITE_BEETLES || value == ElementType.BLACK_BEETLES)
                {
                    lastType = type;
                    type = value;
                } else if (type == ElementType.WHITE_BEETLES || type == ElementType.BLACK_BEETLES)
                {
                    type = lastType;
                }
                else
                {
                    type = value;
                    lastType = value;
                }
                fillImage();
            }
        }

        private void fillImage ()
        {
            string imageUri = "ms-appx:///Assets";

            switch (this.markType)
            {
                case Marktypes.VOID:
                    border.Stroke = baseBorderColor;
                    break;
                case Marktypes.ACCESSIBLE:
                    GameBoardGrid.Children.Remove(border);
                    border.Stroke = accessibleBorderColor;
                    GameBoardGrid.Children.Add(border);
                    break;
                case Marktypes.INACCESSIBLE:
                    GameBoardGrid.Children.Remove(border);
                    border.Stroke = inaccessibleBorderColor;
                    GameBoardGrid.Children.Add(border);
                    break;
                case Marktypes.SELECTED:
                    GameBoardGrid.Children.Remove(border);
                    border.Stroke = selectedBorderColor;
                    GameBoardGrid.Children.Add(border);
                    break;
                    
            }

            switch (this.type)
            {
                case ElementType.WHITE_QUEEN: imageUri += "/White/Queen.png"; break;
                case ElementType.BLACK_QUEEN: imageUri += "/Black/Queen.png"; break;
                case ElementType.WHITE_ANT: imageUri += "/White/Ant.png"; break;
                case ElementType.BLACK_ANT: imageUri += "/Black/Ant.png"; break;
                case ElementType.WHITE_HOPPER: imageUri += "/White/GrassHopper.png"; break;
                case ElementType.BLACK_HOPPER: imageUri += "/Black/GrassHopper.png"; break;
                case ElementType.WHITE_SPIDER: imageUri += "/White/Spider.png"; break;
                case ElementType.BLACK_SPIDER: imageUri += "/Black/Spider.png"; break;
                case ElementType.WHITE_BEETLES: imageUri += "/White/Beetles.png"; break;
                case ElementType.BLACK_BEETLES: imageUri += "/Black/Beetles.png"; break;
                case ElementType.VOID:
                    switch (this.markType)
                    {
                        case Marktypes.VOID:
                        case Marktypes.SELECTED:
                            this.polygon.Fill = baseColor;
                            break;
                        case Marktypes.ACCESSIBLE:
                            this.polygon.Fill = accessibleColor;
                            break;
                        case Marktypes.INACCESSIBLE:
                            this.polygon.Fill = inaccessibleColor;
                            break;
                    } 
                    return;
            }

            this.polygon.Fill = new ImageBrush
            {
                ImageSource = new BitmapImage(new Uri(imageUri))
            };
        }

        public Marktypes MarkType
        {
            get => markType;
            set
            {
                markType = value;
                fillImage();
            }
        }
        public PlayerColor getElementColor()
        {
            switch (Type)
            {
                case ElementType.WHITE_QUEEN:
                case ElementType.WHITE_ANT:
                case ElementType.WHITE_HOPPER:
                case ElementType.WHITE_SPIDER:
                case ElementType.WHITE_BEETLES:
                    return PlayerColor.White;
                case ElementType.BLACK_QUEEN:
                case ElementType.BLACK_ANT:
                case ElementType.BLACK_HOPPER:
                case ElementType.BLACK_SPIDER:
                case ElementType.BLACK_BEETLES:
                    return PlayerColor.Black;
                default:
                    return PlayerColor.None;
            }
        }
    }

    public class FieldElement : Element, IEquatable<FieldElement>
    {
        private int i;
        private int j;
        public FieldElement(Action<Element> tappCallback, ElementType type, dynamic GameBoardGrid, int i, int j, double X, double Y, double width, double height) :
            base(tappCallback, type, (Object)GameBoardGrid, X, Y, width, height)
        {
            this.i = i;
            this.j = j;
        }


        public int J { get => j; }
        public int I { get => i; }

        public bool Equals(FieldElement other)
        {
            return this.i == other.i && this.j == other.j;
        }
    }
    public class PlayerElement : Element
    {
        public readonly PlayerColor playerColor;

        public PlayerElement(Action<Element> tappCallback, ElementType type, dynamic PlayerElementsGrid, PlayerColor playerColor, double X, double Y, double width, double height) :
            base(tappCallback, type, (Object)PlayerElementsGrid, X, Y, width, height)
        {
            this.playerColor = playerColor;
        }

       
    }
}
