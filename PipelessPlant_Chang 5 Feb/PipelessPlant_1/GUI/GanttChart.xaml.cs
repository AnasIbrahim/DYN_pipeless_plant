using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MULTIFORM_PCS.GUI
{
    /// <summary>
    /// Interaktionslogik für GanttChart.xaml
    /// </summary>
    public partial class GanttChart : UserControl
    {
        private double[] zoomSteps = new double[] { 0.25d, 0.5d, 0.75d, 1, 2, 3, 4, 5, 7.5d, 10, 12.5d, 15 };
        private int curZoom = 4;
        private double[] sizesBars = new double[] { 24, 36, 48, 60, 72 };
        private int curBarSize = 1;

        private Line curExecLine;
        private double offset = 20;
        private double timeUnitWidth = 5;
        private double rowHeight = 36; //should be even
        private bool focusOnTimeLine = false;
        private bool loaded = false;
        private double maxTime;
        private TimeSpan curT = TimeSpan.FromMilliseconds(0);
        private Datastructure.Schedule.DetailedProductionPlan d;
        private Datastructure.Schedule.Schedule s;

        public GanttChart()
        {
            InitializeComponent(); 
        }

        public void updateExecutionTime(TimeSpan t)
        {
            curT = t;
            int h = t.Hours;
            int m = t.Minutes;
            int s = t.Seconds;
            int milis = t.Milliseconds;

            string time = "";
            if (h >= 10)
            {
                time += h + ":";
            }
            else
            {
                time += "0" + h + ":";
            }
            if (m >= 10)
            {
                time += m + ":";
            }
            else
            {
                time += "0" + m + ":";
            }
            if (s >= 10)
            {
                time += s + ":";
            }
            else
            {
                time += "0" + s + ":";
            }
            if (milis < 10)
            {
                time += "00" + milis;
            }
            else if (milis < 100)
            {
                time += "0" + milis;
            }
            else
            {
                time += milis;
            }

            textBlockTime.Text = time;

            Canvas.SetLeft(curExecLine, offset + timeUnitWidth/1000*t.TotalMilliseconds);

            focus();
        }

        public void drawSchedule(Datastructure.Schedule.DetailedProductionPlan d, Datastructure.Schedule.Schedule s)
        {
            this.d = d;
            this.s = s;

            drawSchedule();
        }

        public void drawSchedule()
        {

            gridResourceLabels.Children.Clear();
            canvasDrawGantt.Children.Clear();

            if (s != null && d != null)
            {
                //double rowHeight = 36; //should be even
                int resources = 0;
                for (int i = 0; i < s.RawData.agvUsed; i++)
                {
                    RowDefinition def = new RowDefinition();
                    def.Height = new GridLength(rowHeight);
                    gridResourceLabels.RowDefinitions.Add(def);
                    TextBlock agvTextBlock = new TextBlock();
                    agvTextBlock.Text = "AGV " + (i + 1);
                    agvTextBlock.TextAlignment = TextAlignment.Right;
                    agvTextBlock.VerticalAlignment = VerticalAlignment.Stretch;
                    agvTextBlock.FontSize = 20;
                    agvTextBlock.FontWeight = FontWeights.Bold;
                    agvTextBlock.Margin = new Thickness(2);
                    gridResourceLabels.Children.Add(agvTextBlock);
                    Grid.SetRow(agvTextBlock, i);
                    resources++;
                }
                for (int i = 0; i < s.RawData.storageCount; i++)
                {
                    RowDefinition def = new RowDefinition();
                    def.Height = new GridLength(rowHeight);
                    gridResourceLabels.RowDefinitions.Add(def);
                    TextBlock StorageTextBlock = new TextBlock();
                    StorageTextBlock.Text = "StorageStation " + (i + 1);
                    StorageTextBlock.TextAlignment = TextAlignment.Right;
                    StorageTextBlock.VerticalAlignment = VerticalAlignment.Stretch;
                    StorageTextBlock.FontSize = 20;
                    StorageTextBlock.FontWeight = FontWeights.Bold;
                    StorageTextBlock.Margin = new Thickness(2);
                    gridResourceLabels.Children.Add(StorageTextBlock);
                    Grid.SetRow(StorageTextBlock, i + s.RawData.agvUsed);
                    resources++;
                }
                for (int i = 0; i < s.RawData.fillingCount; i++)
                {
                    RowDefinition def = new RowDefinition();
                    def.Height = new GridLength(rowHeight);
                    gridResourceLabels.RowDefinitions.Add(def);
                    TextBlock fillingTextBlock = new TextBlock();
                    fillingTextBlock.Text = "FillingStation " + (i + 1);
                    fillingTextBlock.TextAlignment = TextAlignment.Right;
                    fillingTextBlock.VerticalAlignment = VerticalAlignment.Stretch;
                    fillingTextBlock.FontSize = 20;
                    fillingTextBlock.FontWeight = FontWeights.Bold;
                    fillingTextBlock.Margin = new Thickness(2);
                    gridResourceLabels.Children.Add(fillingTextBlock);
                    Grid.SetRow(fillingTextBlock, i + s.RawData.agvUsed + s.RawData.storageCount);
                    resources++;
                }
                for (int i = 0; i < s.RawData.mixingCount; i++)
                {
                    RowDefinition def = new RowDefinition();
                    def.Height = new GridLength(rowHeight);
                    gridResourceLabels.RowDefinitions.Add(def);
                    TextBlock mixingTextBlock = new TextBlock();
                    mixingTextBlock.Text = "MixingStation " + (i + 1);
                    mixingTextBlock.TextAlignment = TextAlignment.Right;
                    mixingTextBlock.VerticalAlignment = VerticalAlignment.Stretch;
                    mixingTextBlock.FontSize = 20;
                    mixingTextBlock.FontWeight = FontWeights.Bold;
                    mixingTextBlock.Margin = new Thickness(2);
                    gridResourceLabels.Children.Add(mixingTextBlock);
                    Grid.SetRow(mixingTextBlock, i + s.RawData.agvUsed + s.RawData.storageCount + s.RawData.fillingCount);
                    resources++;
                }
                for (int i = 0; i < s.RawData.recipesCount; i++)
                {
                    RowDefinition def = new RowDefinition();
                    def.Height = new GridLength(rowHeight);
                    gridResourceLabels.RowDefinitions.Add(def);
                    TextBlock recipeTextBlock = new TextBlock();
                    recipeTextBlock.Text = "Recipe " + (i + 1);
                    recipeTextBlock.TextAlignment = TextAlignment.Right;
                    recipeTextBlock.VerticalAlignment = VerticalAlignment.Stretch;
                    recipeTextBlock.FontSize = 20;
                    recipeTextBlock.FontWeight = FontWeights.Bold;
                    recipeTextBlock.Margin = new Thickness(2);
                    gridResourceLabels.Children.Add(recipeTextBlock);
                    Grid.SetRow(recipeTextBlock, i + s.RawData.agvUsed + s.RawData.fillingCount + s.RawData.mixingCount + s.RawData.storageCount);
                    resources++;
                }

                legendRow.Height = new GridLength(resources * rowHeight);
                canvasDrawGantt.Height = 15 + 65 - 19 + resources * rowHeight;
                float makespan = 0;
                for (int i = 0; i < s.BestSchedule.Count; i++)
                {
                    float curEndTime = float.Parse(s.BestSchedule[i][3]);
                    if (curEndTime > makespan)
                    {
                        makespan = curEndTime;
                    }
                }
                canvasDrawGantt.Width = makespan * timeUnitWidth + 2 * offset;

                loaded = true;
                maxTime = makespan * timeUnitWidth;

                Line xAxis = new Line();
                xAxis.Stroke = Brushes.Black;
                xAxis.StrokeThickness = 3;
                canvasDrawGantt.Children.Add(xAxis);
                Canvas.SetLeft(xAxis, 0);
                Canvas.SetTop(xAxis, resources * rowHeight);
                xAxis.X1 = 0;
                xAxis.Y1 = 0;
                xAxis.X2 = makespan * timeUnitWidth + 2 * offset;
                xAxis.Y2 = 0;

                for (int i = 0; i < resources; i++)
                {
                    Line vertical = new Line();
                    vertical.Stroke = Brushes.Black;
                    vertical.StrokeThickness = 1;
                    canvasDrawGantt.Children.Add(vertical);
                    Canvas.SetLeft(vertical, 0);
                    Canvas.SetTop(vertical, i * rowHeight + rowHeight / 2);
                    vertical.X1 = 0;
                    vertical.Y1 = 0;
                    vertical.X2 = makespan * timeUnitWidth + 2 * offset;
                    vertical.Y2 = 0;
                    vertical.StrokeDashArray.Add(2);//HIER HÜBSCH MACHEN!
                    vertical.StrokeDashArray.Add(5);
                }

                for (int i = 0; i < makespan; i++)
                {
                    Line l = new Line();
                    l.Stroke = Brushes.Black;
                    canvasDrawGantt.Children.Add(l);
                    l.X1 = 0;
                    l.Y1 = 0;
                    l.X2 = 0;
                    if (timeUnitWidth < 0.75d)
                    {
                        if (i % 100 == 0)
                        {
                            TextBlock mark = new TextBlock();
                            mark.Text = i.ToString();
                            mark.TextAlignment = TextAlignment.Center;
                            mark.Width = 100;
                            canvasDrawGantt.Children.Add(mark);
                            Canvas.SetTop(mark, resources * rowHeight + 35 + 5);
                            Canvas.SetLeft(mark, offset + i * timeUnitWidth - 50);

                            l.Y2 = 35;
                            Line lbig = new Line();
                            lbig.Stroke = Brushes.Black;
                            canvasDrawGantt.Children.Add(lbig);
                            lbig.X1 = 0;
                            lbig.Y1 = 0;
                            lbig.X2 = 0;
                            lbig.Y2 = resources * rowHeight;
                            lbig.StrokeDashArray.Add(2);//HIER HÜBSCH MACHEN!
                            lbig.StrokeDashArray.Add(1);
                            Canvas.SetTop(lbig, 0);
                            Canvas.SetLeft(lbig, offset + i * timeUnitWidth);
                        }
                        else if (i % 50 == 0)
                        {
                            l.Y2 = 30;
                        }
                    }
                    else
                    {
                        if (i % 10 == 0)
                        {
                            if (timeUnitWidth >= 3 || i % 50 == 0)
                            {
                                TextBlock mark = new TextBlock();
                                mark.Text = i.ToString();
                                mark.TextAlignment = TextAlignment.Center;
                                mark.Width = 100;
                                canvasDrawGantt.Children.Add(mark);
                                Canvas.SetTop(mark, resources * rowHeight + 35 + 5);
                                Canvas.SetLeft(mark, offset + i * timeUnitWidth - 50);
                            }

                            l.Y2 = 35;
                            Line lbig = new Line();
                            lbig.Stroke = Brushes.Black;
                            canvasDrawGantt.Children.Add(lbig);
                            lbig.X1 = 0;
                            lbig.Y1 = 0;
                            lbig.X2 = 0;
                            lbig.Y2 = resources * rowHeight;
                            lbig.StrokeDashArray.Add(2);//HIER HÜBSCH MACHEN!
                            lbig.StrokeDashArray.Add(1);
                            Canvas.SetTop(lbig, 0);
                            Canvas.SetLeft(lbig, offset + i * timeUnitWidth);

                        }
                        else if (i % 5 == 0)
                        {
                            l.Y2 = 30;

                            if (timeUnitWidth >= 10 && i % 10 != 0)
                            {
                                TextBlock mark = new TextBlock();
                                mark.Text = i.ToString();
                                mark.TextAlignment = TextAlignment.Center;
                                mark.Width = 100;
                                canvasDrawGantt.Children.Add(mark);
                                Canvas.SetTop(mark, resources * rowHeight + 35 + 5);
                                Canvas.SetLeft(mark, offset + i * timeUnitWidth - 50);
                            }
                        }
                        else
                        {
                            if (timeUnitWidth >= 2)
                            {
                                l.Y2 = 20;
                            }
                            else
                            {
                                canvasDrawGantt.Children.Remove(l);
                                continue;
                            }
                        }
                    }
                    Canvas.SetTop(l, resources * rowHeight);
                    Canvas.SetLeft(l, offset + i * timeUnitWidth);
                }

                for (int i = 0; i < d.OverallStationProductionPlan.Count; i++)
                {
                    int unit = -1;
                    if (d.OverallStationProductionPlan[i].res == Datastructure.Schedule.DetailedProductionPlan.RESOURCE.STORAGESTATION)
                    {
                        unit = 0;
                    }
                    else if (d.OverallStationProductionPlan[i].res == Datastructure.Schedule.DetailedProductionPlan.RESOURCE.COLORSTATION1)
                    {
                        unit = 1;
                    }
                    else if (d.OverallStationProductionPlan[i].res == Datastructure.Schedule.DetailedProductionPlan.RESOURCE.COLORSTATION2)
                    {
                        unit = 2;
                    }
                    else if (d.OverallStationProductionPlan[i].res == Datastructure.Schedule.DetailedProductionPlan.RESOURCE.MIXINGSTATION)
                    {
                        unit = 3;
                    }
                    else if (d.OverallStationProductionPlan[i].res == Datastructure.Schedule.DetailedProductionPlan.RESOURCE.VESSEL1)
                    {
                        unit = 4;
                    }
                    else if (d.OverallStationProductionPlan[i].res == Datastructure.Schedule.DetailedProductionPlan.RESOURCE.VESSEL2)
                    {
                        unit = 5;
                    }
                    else if (d.OverallStationProductionPlan[i].res == Datastructure.Schedule.DetailedProductionPlan.RESOURCE.VESSEL3)
                    {
                        unit = 6;
                    }
                    else if (d.OverallStationProductionPlan[i].res == Datastructure.Schedule.DetailedProductionPlan.RESOURCE.VESSEL4)
                    {
                        unit = 7;
                    }
                    else if (d.OverallStationProductionPlan[i].res == Datastructure.Schedule.DetailedProductionPlan.RESOURCE.VESSEL5)
                    {
                        unit = 8;
                    }
                    else if (d.OverallStationProductionPlan[i].res == Datastructure.Schedule.DetailedProductionPlan.RESOURCE.VESSEL6)
                    {
                        unit = 9;
                    }

                    Border taskBorder = new Border();
                    int margin = 2;
                    taskBorder.Height = rowHeight - margin;//quasi margin 2 in oben/unten richtungen!
                    taskBorder.Width = (d.OverallStationProductionPlan[i].endTime - d.OverallStationProductionPlan[i].startTime) * timeUnitWidth;
                    taskBorder.CornerRadius = new CornerRadius(5);
                    canvasDrawGantt.Children.Add(taskBorder);
                    Canvas.SetLeft(taskBorder, offset + d.OverallStationProductionPlan[i].startTime * timeUnitWidth);
                    Canvas.SetTop(taskBorder, (unit + s.RawData.agvUsed) * rowHeight + margin / 2);//quasi margin 2 in oben/unten richtungen!
                    taskBorder.BorderBrush = Brushes.Black;
                    taskBorder.BorderThickness = new Thickness(2);

                    d.OverallStationProductionPlan[i].visual = taskBorder;

                    if (d.OverallStationProductionPlan[i].recipeID == 1)
                    {
                        taskBorder.Background = (Brush)this.FindResource("LGBOrange");
                    }
                    else if (d.OverallStationProductionPlan[i].recipeID == 2)
                    {
                        taskBorder.Background = (Brush)this.FindResource("LGBRed");
                    }
                    else if (d.OverallStationProductionPlan[i].recipeID == 3)
                    {
                        taskBorder.Background = (Brush)this.FindResource("LGBBlue");
                    }
                    else if (d.OverallStationProductionPlan[i].recipeID == 4)
                    {
                        taskBorder.Background = (Brush)this.FindResource("LGBGreen");
                    }
                    else if (d.OverallStationProductionPlan[i].recipeID == 5)
                    {
                        taskBorder.Background = (Brush)this.FindResource("LGBPurple");
                    }
                    else if (d.OverallStationProductionPlan[i].recipeID == 6)
                    {
                        taskBorder.Background = (Brush)this.FindResource("LGBYellow");
                    }

                    /**Grid g = new Grid();
                    g.VerticalAlignment = VerticalAlignment.Stretch;
                    g.HorizontalAlignment = HorizontalAlignment.Stretch;
                    taskBorder.Child = g;*/

                    /**Label indi = new Label();
                    indi.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                    indi.VerticalContentAlignment = System.Windows.VerticalAlignment.Center;
                    indi.VerticalAlignment = System.Windows.VerticalAlignment.Center;
                    indi.HorizontalContentAlignment = System.Windows.HorizontalAlignment.Center;
                    indi.Content = "W";
                    indi.FontSize = 10;
                    g.Children.Add(indi);

                    d.OverallStationProductionPlan[i].indicator = indi;*/

                    /**TextBlock TextBlockTaskID = new TextBlock();
                    TextBlockTaskID.TextAlignment = TextAlignment.Center;
                    TextBlockTaskID.VerticalAlignment = VerticalAlignment.Stretch;
                    TextBlockTaskID.HorizontalAlignment = HorizontalAlignment.Stretch;
                    TextBlockTaskID.Text = d.OverallStationProductionPlan[i].taskID;
                    TextBlockTaskID.FontSize = 20;*/
                    //TextBlockTaskID.FontWeight = FontWeights.Bold;
                    //g.Children.Add(TextBlockTaskID);
                    taskBorder.ToolTip = d.OverallStationProductionPlan[i].operation.ToString() + " (" + d.OverallStationProductionPlan[i].taskID + ")\nDURATION: " + (d.OverallStationProductionPlan[i].endTime -d.OverallStationProductionPlan[i].startTime)+ "\nPrecondition recipe: " + (d.OverallStationProductionPlan[i].preconditionSameRecipe != null ? d.OverallStationProductionPlan[i].preconditionSameRecipe.taskID : "none") + "\nPrecondition resource: " + (d.OverallStationProductionPlan[i].preconditionSameResource != null ? d.OverallStationProductionPlan[i].preconditionSameResource.taskID : "none") + "\nSynchronization: " + (d.OverallStationProductionPlan[i].synchCondition != null ? d.OverallStationProductionPlan[i].synchCondition.taskID : "none");
                }

                for (int i = 0; i < d.OverallAGVProductionPlan.Count; i++)
                {
                    int rowToPut = 0;
                    if (d.OverallAGVProductionPlan[i].res == Datastructure.Schedule.DetailedProductionPlan.RESOURCE.AGV1)
                    {
                        rowToPut = 0;
                    }
                    else if (d.OverallAGVProductionPlan[i].res == Datastructure.Schedule.DetailedProductionPlan.RESOURCE.AGV2)
                    {
                        rowToPut = 1;
                    }
                    else if (d.OverallAGVProductionPlan[i].res == Datastructure.Schedule.DetailedProductionPlan.RESOURCE.AGV3)
                    {
                        rowToPut = 2;
                    }
                    else if (d.OverallAGVProductionPlan[i].res == Datastructure.Schedule.DetailedProductionPlan.RESOURCE.AGV4)
                    {
                        rowToPut = 3;
                    }
                    else if (d.OverallAGVProductionPlan[i].res == Datastructure.Schedule.DetailedProductionPlan.RESOURCE.AGV5)
                    {
                        rowToPut = 4;
                    }

                    Border agvTaskBorder = new Border();
                    int margin = 2;
                    agvTaskBorder.Height = rowHeight - margin;//quasi margin 2 in oben/unten richtungen!
                    agvTaskBorder.Width = (d.OverallAGVProductionPlan[i].endTime - d.OverallAGVProductionPlan[i].startTime) * timeUnitWidth;
                    agvTaskBorder.CornerRadius = new CornerRadius(5);
                    canvasDrawGantt.Children.Add(agvTaskBorder);
                    Canvas.SetLeft(agvTaskBorder, offset + d.OverallAGVProductionPlan[i].startTime * timeUnitWidth);
                    Canvas.SetTop(agvTaskBorder, rowToPut * rowHeight + margin / 2);//quasi margin 2 in oben/unten richtungen!
                    agvTaskBorder.BorderBrush = Brushes.Black;
                    agvTaskBorder.BorderThickness = new Thickness(2);

                    d.OverallAGVProductionPlan[i].visual = agvTaskBorder;

                    /**Grid g = new Grid();
                    g.VerticalAlignment = VerticalAlignment.Stretch;
                    g.HorizontalAlignment = HorizontalAlignment.Stretch;
                    agvTaskBorder.Child = g;*/

                    /**Label indi = new Label();
                    indi.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                    indi.VerticalContentAlignment = System.Windows.VerticalAlignment.Center;
                    indi.VerticalAlignment = System.Windows.VerticalAlignment.Center;
                    indi.HorizontalContentAlignment = System.Windows.HorizontalAlignment.Center;
                    indi.Content = "W";
                    indi.FontSize = 10;
                    g.Children.Add(indi);

                    d.OverallAGVProductionPlan[i].indicator = indi;*/

                    if (d.OverallAGVProductionPlan[i].recipeID == 1)
                    {
                        agvTaskBorder.Background = (Brush)this.FindResource("LGBOrange");
                    }
                    else if (d.OverallAGVProductionPlan[i].recipeID == 2)
                    {
                        agvTaskBorder.Background = (Brush)this.FindResource("LGBRed");
                    }
                    else if (d.OverallAGVProductionPlan[i].recipeID == 3)
                    {
                        agvTaskBorder.Background = (Brush)this.FindResource("LGBBlue");
                    }
                    else if (d.OverallAGVProductionPlan[i].recipeID == 4)
                    {
                        agvTaskBorder.Background = (Brush)this.FindResource("LGBGreen");
                    }
                    else if (d.OverallAGVProductionPlan[i].recipeID == 5)
                    {
                        agvTaskBorder.Background = (Brush)this.FindResource("LGBPurple");
                    }
                    else if (d.OverallAGVProductionPlan[i].recipeID == 6)
                    {
                        agvTaskBorder.Background = (Brush)this.FindResource("LGBYellow");
                    }

                    if (d.OverallAGVProductionPlan[i].operation == Datastructure.Schedule.DetailedProductionPlan.ELEMENTS.WAIT)
                    {
                        agvTaskBorder.Opacity = 0.5d;
                    }

                    /**TextBlock TextBlockAGVTaskID = new TextBlock();
                    TextBlockAGVTaskID.TextAlignment = TextAlignment.Center;
                    TextBlockAGVTaskID.FontWeight = FontWeights.Bold;
                    TextBlockAGVTaskID.FontSize = 20;
                    TextBlockAGVTaskID.VerticalAlignment = VerticalAlignment.Stretch;
                    TextBlockAGVTaskID.HorizontalAlignment = HorizontalAlignment.Stretch;
                    //TextBlockAGVTaskID.Text = d.OverallAGVProductionPlan[i].taskID;
                    g.Children.Add(TextBlockAGVTaskID);*/
                    agvTaskBorder.ToolTip = d.OverallAGVProductionPlan[i].operation.ToString() + " (" + d.OverallAGVProductionPlan[i].taskID + ")\nDURATION: " + (d.OverallAGVProductionPlan[i].endTime - d.OverallAGVProductionPlan[i].startTime) + "\nPrecondition recipe: " + (d.OverallAGVProductionPlan[i].preconditionSameRecipe != null ? d.OverallAGVProductionPlan[i].preconditionSameRecipe.taskID : "none") + "\nPrecondition resource: " + (d.OverallAGVProductionPlan[i].preconditionSameResource != null ? d.OverallAGVProductionPlan[i].preconditionSameResource.taskID : "none") + "\nSynchronization: " + (d.OverallAGVProductionPlan[i].synchCondition != null ? d.OverallAGVProductionPlan[i].synchCondition.taskID : "none");
                }

                curExecLine = new Line();
                curExecLine.Stroke = Brushes.Red;
                curExecLine.StrokeThickness = 2.0d;
                canvasDrawGantt.Children.Add(curExecLine);
                curExecLine.X1 = 0;
                curExecLine.Y1 = 0;
                curExecLine.X2 = 0;
                curExecLine.Y2 = resources * rowHeight + 20;
                Canvas.SetTop(curExecLine, 0);
                Canvas.SetLeft(curExecLine, offset);
            }
        }

        private void focus()
        {
            if (focusOnTimeLine && loaded)
            {
                textBlockTime.Foreground = Brushes.Red;
                scrollviewerGantt.ScrollToHorizontalOffset(scrollviewerGantt.ScrollableWidth / (maxTime + 2 * offset) * (timeUnitWidth / 1000 * curT.TotalMilliseconds));
                    //akt. Wert: offset + timeUnitWidth/1000*curT.TotalMilliseconds
            }
            else
            {
                textBlockTime.Foreground = Brushes.Black;
            }
        }

        private void textBlockTime_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            focusOnTimeLine = !focusOnTimeLine;
            focus();
        }

        private void buttonZoomOut_Click(object sender, RoutedEventArgs e)
        {
            curZoom = curZoom - 1;
            if (curZoom < 0)
            {
                curZoom = 0;
            }

            changeZoomStep(zoomSteps[curZoom]);
            drawSchedule();
        }

        private void buttonZoomIn_Click(object sender, RoutedEventArgs e)
        {
            curZoom = curZoom + 1;
            if (curZoom >= zoomSteps.Length)
            {
                curZoom = zoomSteps.Length - 1;
            }

            changeZoomStep(zoomSteps[curZoom]);
            drawSchedule();
        }

        private void buttonIncrBars_Click(object sender, RoutedEventArgs e)
        {
            curBarSize = curBarSize + 1;
            if (curBarSize >= sizesBars.Length)
            {
                curBarSize = sizesBars.Length - 1;
            }

            changeRowHeight(sizesBars[curBarSize]);
            drawSchedule();
        }

        private void buttonDecrBars_Click(object sender, RoutedEventArgs e)
        {
            curBarSize = curBarSize - 1;
            if (curBarSize < 0)
            {
                curBarSize = 0;
            }

            changeRowHeight(sizesBars[curBarSize]);
            drawSchedule();
        }

        private void changeZoomStep(double zoomStep)
        {
            this.timeUnitWidth = zoomStep;
        }
        private void changeRowHeight(double rowH)
        {
            this.rowHeight = rowH;
        }
    }
}
