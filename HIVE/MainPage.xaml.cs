using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;

namespace HIVE
{

    public sealed partial class MainPage : Page
    {
        private GameBoard board;

        private Player whitePlayer;
        private Player blackPlayer;
        private Player currentPlayer;
        private List<PlayerElement> whitePlayerElements;
        private List<PlayerElement> blackPlayerElements;

        private bool firstRound;
        private Element choosedElement;
        private bool win;

        private int queenCounter_White, queenCounter_Black;
        private int spiderCounter_White, spiderCounter_Black;
        private int beetlesCounter_White, beetlesCounter_Black;
        private int antCounter_White, antCounter_Black;
        private int hopperCounter_White, hopperCounter_Black;

        public MainPage()
        {
            this.InitializeComponent();
            this.Loaded += new RoutedEventHandler(Page_Loaded);
        }

        private void Page_Loaded(object sender, RoutedEventArgs e) => rebuild();
        private void reset(object sender, RoutedEventArgs e) => rebuild();
        private void reset(IUICommand command) => rebuild();

        private void rebuild ()
        {
            board = new GameBoard(GameBoardGrid, FieldElementTapped);
            MakePlayerBoards();
            whitePlayer = new Player(PlayerColor.White);
            blackPlayer = new Player(PlayerColor.Black);
            currentPlayer = whitePlayer; //first player
            firstRound = true;
            win = false;
            choosedElement = null;
            queenCounter_White = 1;
            queenCounter_Black = 1;
            spiderCounter_White = 2;
            spiderCounter_Black = 2;
            beetlesCounter_White = 2;
            beetlesCounter_Black = 2;
            antCounter_White = 3;
            antCounter_Black = 3;
            hopperCounter_White = 3;
            hopperCounter_Black = 3;
    }


        private void MakePlayerBoards()
        {
            this.whitePlayerElements = new List<PlayerElement>();
            this.blackPlayerElements = new List<PlayerElement>();

            ElementType[] types =
            {
                ElementType.WHITE_QUEEN,
                ElementType.WHITE_SPIDER,
                ElementType.WHITE_BEETLES,
                ElementType.WHITE_ANT,
                ElementType.WHITE_HOPPER,

                ElementType.BLACK_QUEEN,
                ElementType.BLACK_SPIDER,
                ElementType.BLACK_BEETLES,
                ElementType.BLACK_ANT,
                ElementType.BLACK_HOPPER,
            };

            int count = types.Length/2;
            double temp = WhitePlayerElementsGrid.ActualHeight / (count * 3 + 1) * 2;
            dynamic size = new
            {
                width = temp,
                height = temp
            };


            for (int i = 0; i < count; i++)
            {
                double X = (WhitePlayerElementsGrid.ActualWidth - size.width) / 2;
                double Y = (size.height / 2) + i * size.height * 1.5;

                Action<Element> callback = (Element x) => PlayerElementTapped(x);
                PlayerElement newWhiteElement = new PlayerElement(callback,
                                                                  types[i],
                                                                  WhitePlayerElementsGrid, 
                                                                  PlayerColor.White,
                                                                  X,
                                                                  Y,
                                                                  size.width,
                                                                  size.width);
                whitePlayerElements.Add(newWhiteElement);

                PlayerElement newBlackElement = new PlayerElement(callback,
                                                                  types[i + count],
                                                                  BlackPlayerElementsGrid,
                                                                  PlayerColor.Black,
                                                                  X,
                                                                  Y,
                                                                  size.width,
                                                                  size.width);
                blackPlayerElements.Add(newBlackElement);
            }
        }

        private async System.Threading.Tasks.Task nextPlayerAsync ()
        {
            if (currentPlayer.Equals(whitePlayer)) currentPlayer = blackPlayer;
            else currentPlayer = whitePlayer;
            choosedElement = null;
            PlayerColor winner = board.checkWin();
            switch (winner)
            {
                case PlayerColor.White:
                    win = true;
                    MessageDialog messageDialog = new MessageDialog("The white player won!\n\nDo you want to restart?");
                    messageDialog.Commands.Add(new UICommand("Yes", reset));
                    messageDialog.Commands.Add(new UICommand("No"));
                    await messageDialog.ShowAsync();
                    break;
                case PlayerColor.Black:
                    win = true;
                    messageDialog = new MessageDialog("The black player won!\n\nDo you want to restart?");
                    messageDialog.Commands.Add(new UICommand("Yes", reset));
                    messageDialog.Commands.Add(new UICommand("No"));
                    await messageDialog.ShowAsync();
                    break;
                default:
                    break;
            }
        }

        private void setCounter (PlayerElement element)
        {
            switch (element.Type)
            {
                case ElementType.WHITE_QUEEN:   if (--queenCounter_White == 0) element.MarkType = Marktypes.INACCESSIBLE; break;
                case ElementType.BLACK_QUEEN:   if (--queenCounter_Black == 0) element.MarkType = Marktypes.INACCESSIBLE; break;
                case ElementType.WHITE_ANT:     if (--antCounter_White == 0) element.MarkType = Marktypes.INACCESSIBLE; break;
                case ElementType.BLACK_ANT:     if (--antCounter_Black == 0) element.MarkType = Marktypes.INACCESSIBLE; break;
                case ElementType.WHITE_HOPPER:  if (--hopperCounter_White == 0) element.MarkType = Marktypes.INACCESSIBLE; break;
                case ElementType.BLACK_HOPPER:  if (--hopperCounter_Black == 0) element.MarkType = Marktypes.INACCESSIBLE; break;
                case ElementType.WHITE_SPIDER:  if (--spiderCounter_White == 0) element.MarkType = Marktypes.INACCESSIBLE; break;
                case ElementType.BLACK_SPIDER:  if (--spiderCounter_Black == 0) element.MarkType = Marktypes.INACCESSIBLE; break;
                case ElementType.WHITE_BEETLES: if (--beetlesCounter_White == 0) element.MarkType = Marktypes.INACCESSIBLE; break;
                case ElementType.BLACK_BEETLES: if (--beetlesCounter_Black == 0) element.MarkType = Marktypes.INACCESSIBLE; break;
            }
        }

        private void FieldElementTapped(Element element)
        {
            if (win) return;

            FieldElement tappedElement = (FieldElement)element;

            if (choosedElement is FieldElement &&  tappedElement.Equals((FieldElement)choosedElement))
            {
                board.clearMarks();
                choosedElement = null;
            }
            else if (firstRound)
            {
                if (tappedElement.MarkType == Marktypes.ACCESSIBLE)
                {
                    if (currentPlayer.Equals(whitePlayer))
                    {
                        board.summonElement(tappedElement, (PlayerElement)choosedElement);
                        setCounter((PlayerElement)choosedElement);
                        nextPlayerAsync();
                    }
                    else
                    {
                        board.summonElement(tappedElement, (PlayerElement)choosedElement);
                        setCounter((PlayerElement)choosedElement);
                        nextPlayerAsync();
                        firstRound = false;
                    }
                } 
            } 
            else
            {
                if(this.board.isElementUsed(tappedElement) && tappedElement.getElementColor () == currentPlayer.Color)
                {
                    if (tappedElement.MarkType == Marktypes.ACCESSIBLE)
                    {
                        board.step(tappedElement, (FieldElement)choosedElement);
                        nextPlayerAsync();
                    }
                    else if (tappedElement.MarkType == Marktypes.INACCESSIBLE)
                    {
                        return;
                    }
                    else
                    {
                        this.choosedElement = tappedElement;
                        board.markStepableFields(tappedElement);
                    }
                }
                else if (tappedElement.MarkType == Marktypes.ACCESSIBLE)
                {
                    if (choosedElement is PlayerElement)
                    {
                        board.summonElement(tappedElement, (PlayerElement)choosedElement);
                        setCounter((PlayerElement)choosedElement);
                        nextPlayerAsync();
                    } 
                    else
                    {
                        board.step(tappedElement, (FieldElement)choosedElement);
                        nextPlayerAsync();
                    }
                }
            }
        }

        private void PlayerElementTapped(Element element)
        {
            if (win) return;

            PlayerElement tappedElement = (PlayerElement)element;
            if (tappedElement.MarkType == Marktypes.INACCESSIBLE) 
            {
                return;
            } 
            else if (choosedElement is PlayerElement && tappedElement == (PlayerElement)choosedElement)
            {
                board.clearMarks();
                choosedElement = null;
            }
            else if (firstRound)
            {
                if (currentPlayer.Equals(whitePlayer) && tappedElement.playerColor == PlayerColor.White)
                {
                    choosedElement = element;
                    board.markMiddleElement();
                    board.selectElement(element);
                } 
                else if (currentPlayer.Equals(blackPlayer) && tappedElement.playerColor == PlayerColor.Black)
                {
                    choosedElement = element;
                    board.markMiddleElementNeighbours();
                    board.selectElement(element);
                }
            } 
            else
            {
                if ((currentPlayer.Equals(whitePlayer) && tappedElement.playerColor == PlayerColor.White) ||
                    (currentPlayer.Equals(blackPlayer) && tappedElement.playerColor == PlayerColor.Black))
                {
                    choosedElement = element;
                    board.markSummonableElements(tappedElement);
                    board.selectElement(element);
                }
            }

        }

    }
}
