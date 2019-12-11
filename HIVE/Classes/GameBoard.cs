using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIVE
{
    class GameBoard
    {
        private List<List<FieldElement>> elements;
        private List<FieldElement> usedElements;
        private List<Element> markedElements;
        private List<FieldElement> checkedElements;
        private List<dynamic> elementsForCheck;

        private Action<Element> tappCallback;
        private dynamic GameBoardGrid;
        public GameBoard (dynamic GameBoardGrid, Action<Element> tappCallback)
        {
            rebuild(GameBoardGrid, tappCallback);
        }
        private void rebuild(dynamic GameBoardGrid, Action<Element> tappCallback)
        {
            this.elements = new List<List<FieldElement>>();
            this.usedElements = new List<FieldElement>();
            this.markedElements = new List<Element>();
            this.checkedElements = new List<FieldElement>();
            this.elementsForCheck = new List<dynamic>();

            const int wCount = 15;
            const int hCount = 25;
            dynamic size = new
            {
                width = GameBoardGrid.ActualWidth / (wCount * 1.5),
                height = GameBoardGrid.ActualHeight / hCount
            };

            for (int i = 0; i < wCount; i++)
            {
                List<FieldElement> currentList = new List<FieldElement>();
                elements.Add(currentList);
                for (int j = 0; j < hCount * 2 - 1; j++)
                {
                    double X = j % 2 == 0 ? (size.width / 4 * 3 + size.width * 1.5 * i) : (size.width * 1.5 * i);
                    double Y = size.height * j / 2;

                    FieldElement newElement = new FieldElement(tappCallback,
                                                                ElementType.VOID,
                                                                GameBoardGrid,
                                                                i,
                                                                j,
                                                                X,
                                                                Y,
                                                                size.width,
                                                                size.height);
                    currentList.Add(newElement);
                }
            }
        }


        //HelpFunctions ------------------------------------------------------------------------------------------------------------
        private FieldElement getMiddelElement()
        {
            return elements[elements.Count / 2][elements[elements.Count / 2].Count / 2 + 1];
        }

        private FieldElement getLeftUpNeighbour(FieldElement element)
        {
            if (element.J % 2 != 0)
            {
                return elements[element.I - 1][element.J - 1];
            }
            else
            {
                return elements[element.I][element.J - 1];
            }
        }
        private FieldElement getLeftDownNeighbour(FieldElement element)
        {
            if (element.J % 2 != 0)
            {
                return elements[element.I - 1][element.J + 1];
            }
            else
            {
                return elements[element.I][element.J + 1];
            }
        }
        private FieldElement getRightUpNeighbour(FieldElement element)
        {
            if (element.J % 2 != 0)
            {
                return elements[element.I][element.J - 1];
            }
            else
            {
                return elements[element.I + 1][element.J - 1];
            }
        }
        private FieldElement getRightDownNeighbour(FieldElement element)
        {
            if (element.J % 2 != 0)
            {
                return elements[element.I][element.J + 1];
            }
            else
            {
                return elements[element.I + 1][element.J + 1];
            }
        }
        private FieldElement getUpNeighbour(FieldElement element)
        {
            return elements[element.I][element.J - 2];
        }
        private FieldElement getDownNeighbour(FieldElement element)
        {
            return elements[element.I][element.J + 2];
        }
        private List<FieldElement> getNeighbours (FieldElement element)
        {
            List<FieldElement> retList = new List<FieldElement>();

            retList.Add(getLeftUpNeighbour(element));
            retList.Add(getLeftDownNeighbour(element));

            retList.Add(getRightUpNeighbour(element));
            retList.Add(getRightDownNeighbour(element));

            retList.Add(getUpNeighbour(element));
            retList.Add(getDownNeighbour(element));

            return retList;
        }

        private void markElement(Element element, Marktypes markType)
        {
            element.MarkType = markType;
            markedElements.Add(element);
        }

        private void markElements(List<Element> elements, Marktypes markType)
        {
            for (int i = 0; i < elements.Count; i++)
                markElement(elements[i], markType);
        }
        private void markElements(List<FieldElement> elements, Marktypes markType)
        {
            for (int i = 0; i < elements.Count; i++)
                markElement((Element)elements[i], markType);
        }

        private bool checkNeighbourToSummon (FieldElement element, PlayerColor colorToCheck)
        {
            List<FieldElement> neighbours = getNeighbours(element);
            for (int i = 0; i < neighbours.Count; i++)
            {
                if (neighbours[i].Type == ElementType.VOID) 
                    continue;

                PlayerColor neighbourColor = neighbours[i].getElementColor();
                if (neighbourColor != PlayerColor.None && neighbourColor != colorToCheck)
                {
                    return false;
                }
            }
            return true;
        }

        private void checkNeighbourToBasicStep(FieldElement neighbour, FieldElement target, int maxStep)
        {
            
            List<FieldElement> neighbours = getNeighbours(neighbour);
            for (int i = 0; i < neighbours.Count; i++)
            {
                if (neighbours[i].Type != ElementType.VOID && !neighbours[i].Equals(target))
                {
                    markElement(neighbour, Marktypes.ACCESSIBLE);
                    if (maxStep != 0)
                    {
                        for (int j = 0; j < neighbours.Count; j++)
                        {
                            if (neighbours[j].Type == ElementType.VOID && !isElementChecked(neighbours[j]))
                                elementsForCheck.Add(new { neighbour = neighbours[j], target = neighbour, maxStep = maxStep - 1 });
                        }
                        return;
                    }
                }
            }
        }

        private void markBasicStepableFields(FieldElement target, int maxStep, bool StepToNeigboursAviavable = false)
        {
            checkedElements.Add(target);
            List<FieldElement> neighbours = getNeighbours(target);
            for (int i = 0; i < neighbours.Count; i++)
            {
                elementsForCheck.Add(new { neighbour = neighbours[i], target = target, maxStep = maxStep - 1});
            }

            while (elementsForCheck.Count > 0)
            {
                dynamic checkingData = elementsForCheck[0];
                if ((checkingData.neighbour.Type == ElementType.VOID || StepToNeigboursAviavable) && !isElementChecked(checkingData.neighbour))
                    checkNeighbourToBasicStep(checkingData.neighbour, checkingData.target, checkingData.maxStep);
                checkedElements.Add(checkingData.target);
                elementsForCheck.RemoveAt(0);
            }
        }

        private void markHoppStepableFields(FieldElement target, Func<FieldElement, FieldElement> nextElement)
        {
            FieldElement elementToCheck = target;
            do
            {
                elementToCheck = nextElement(elementToCheck);
            } while (elementToCheck.Type != ElementType.VOID);

            if (! elementToCheck.Equals (nextElement(target))) {
                markElement(elementToCheck, Marktypes.ACCESSIBLE);
            }
        }

        private void markHoppStepableFields(FieldElement target)
        {
            markHoppStepableFields(target, getUpNeighbour);
            markHoppStepableFields(target, getDownNeighbour);

            markHoppStepableFields(target, getLeftUpNeighbour);
            markHoppStepableFields(target, getLeftDownNeighbour);

            markHoppStepableFields(target, getRightUpNeighbour);
            markHoppStepableFields(target, getRightDownNeighbour);
        }

        private bool isElementChecked(FieldElement element)
        {
            bool retval = this.checkedElements.Find(elem => elem.I == element.I && elem.J == element.J) != null;
            return retval;
        }

        private bool checkQueen(FieldElement element)
        {
            List<FieldElement> neighbours = getNeighbours(element);
            for (int j = 0; j < neighbours.Count; j++)
            {
                if (neighbours[j].Type == ElementType.VOID)
                {
                    return false;
                }
            }
            return true;
        }

        private bool canStep(FieldElement target)
        {
            FieldElement neighbourToCheck = getNeighbours(target).Find(n => n.Type != ElementType.VOID);
            if (neighbourToCheck == null)
            {
                return false;
            }

            checkedElements.Add(target);
            elementsForCheck.Add(neighbourToCheck);
            while (elementsForCheck.Count > 0)
            {
                checkedElements.Add(elementsForCheck[0]);

                List<FieldElement> neighbours = getNeighbours(elementsForCheck[0]);
                for (int i = 0; i < neighbours.Count; i++)
                {
                    if (neighbours[i].Type != ElementType.VOID && ! checkedElements.Contains(neighbours[i]))
                    {
                        elementsForCheck.Add(neighbours[i]);
                    }
                }

                elementsForCheck.RemoveAt(0);
            }

            for (int i = 0; i < usedElements.Count; i++)
            {
                if (!checkedElements.Contains(usedElements[i]))
                {
                    return false;
                }
            }

            return true;
        }
        //Interface ------------------------------------------------------------------------------------------------------------
        public void clearMarks()
        {
            for (int i = 0; i < markedElements.Count; i++)
            {
                markedElements[i].MarkType = Marktypes.VOID;
            }
            markedElements.Clear();
            checkedElements.Clear();
            elementsForCheck.Clear();
        }

        public bool isElementUsed(FieldElement element)
        {
            bool retval = this.usedElements.Find(elem => elem.I == element.I && elem.J == element.J) != null;
            return retval;
        }

        public void selectElement(Element target)
        {
            markElement(target, Marktypes.SELECTED);
        }

        public void markMiddleElement()
        {
            clearMarks();
            FieldElement middleElement = getMiddelElement();
            markElement(middleElement, Marktypes.ACCESSIBLE);
        }

        public void markMiddleElementNeighbours ()
        {
            clearMarks();
            FieldElement middleElement = getMiddelElement();
            List<FieldElement> neighbours = getNeighbours(middleElement);
            markElements(neighbours, Marktypes.ACCESSIBLE);
        }

        public void markSummonableElements (PlayerElement target)
        {
            clearMarks();
            PlayerColor targetColor = target.getElementColor();
            for (int i = 0; i < usedElements.Count; i++)
            {
                if (targetColor == usedElements[i].getElementColor())
                {
                    List<FieldElement> neighbours = getNeighbours(usedElements[i]);
                    for (int j = 0; j < neighbours.Count; j++)
                    {
                        if (neighbours[j].Type != ElementType.VOID) 
                            continue;
                        
                        if (checkNeighbourToSummon (neighbours[j], targetColor))
                        {
                            markElement(neighbours[j], Marktypes.ACCESSIBLE);
                        }
                    }

                }
            }
            selectElement(target);
        }

        public void summonElement (FieldElement target, PlayerElement element)
        {
            target.Type = element.Type;
            usedElements.Add(target);
            clearMarks();
        }

        public void markStepableFields (FieldElement target)
        {
            clearMarks();
            if (! canStep(target))
            {
                markElement(target, Marktypes.INACCESSIBLE);
                return;
            }

            switch (target.Type)
            {
                case ElementType.WHITE_QUEEN:
                case ElementType.BLACK_QUEEN:
                    markBasicStepableFields(target, 1);
                    break;
                case ElementType.WHITE_ANT:
                case ElementType.BLACK_ANT:
                    markBasicStepableFields(target, -1);
                    break;
                case ElementType.WHITE_HOPPER:
                case ElementType.BLACK_HOPPER:
                    markHoppStepableFields(target);
                    break;
                case ElementType.WHITE_SPIDER:
                case ElementType.BLACK_SPIDER:
                    markBasicStepableFields(target, 3);
                    break;
                case ElementType.WHITE_BEETLES:
                case ElementType.BLACK_BEETLES:
                    markBasicStepableFields(target, 1, true);
                    break;
                case ElementType.VOID:
                default:
                    break;
            }
            selectElement(target);
        }

        public void step (FieldElement target, FieldElement element)
        {
            target.Type = element.Type;
            usedElements.Add(target);
            usedElements.Remove(element);
            element.Type = ElementType.VOID;
            clearMarks();
        }

        public PlayerColor checkWin()
        {
            for (int i = 0; i < usedElements.Count; i++)
            {
                if (usedElements[i].Type == ElementType.WHITE_QUEEN)
                {
                    if (checkQueen(usedElements[i])) 
                        return PlayerColor.White;
                }
                else if (usedElements[i].Type == ElementType.BLACK_QUEEN)
                {
                    if (checkQueen(usedElements[i])) 
                        return PlayerColor.Black;
                }
            }
            return PlayerColor.None;
        }

    }
    
}
