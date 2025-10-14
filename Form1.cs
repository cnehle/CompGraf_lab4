using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace lab4._1
{
    public partial class Form1 : Form
    {
        private List<Polygon> polygons = new List<Polygon>();
        private Polygon currentPolygon = null;
        private PointF? testPoint = null;
        private Edge? testEdge1 = null;
        private Edge? testEdge2 = null;
        private PointF rotationCenter;
        private PointF scalingCenter;
        private bool isSettingRotationCenter = false;
        private bool isSettingScalingCenter = false;

        // Режимы работы для динамического ввода
        private enum InteractionMode
        {
            CreatePolygon,
            SetTestPoint,
            SetEdge1,
            SetEdge2,
            SetEdgeForClassification,
            SetPointForClassification
        }
        private InteractionMode currentMode = InteractionMode.CreatePolygon;

        // Для классификации точки относительно ребра
        private Edge? classificationEdge = null;
        private PointF? classificationPoint = null;

        // Для выбора полигонов для проверки и преобразований
        private Polygon selectedPolygonForTest = null;
        private Polygon selectedPolygonForTransform = null;

        // Элементы управления
        private ComboBox cmbTransformType;
        private TextBox txtDx, txtDy, txtAngle, txtScaleX, txtScaleY;
        private Button btnApplyTransform, btnClear, btnCheckPoint, btnCheckEdges, btnClassifyPoint;
        private Label lblStatus;
        private Panel drawingPanel;
        private GroupBox grpTransformParams;
        private Label lblDx, lblDy, lblAngle, lblScaleX, lblScaleY;
        private Button btnSetRotationCenter, btnSetScalingCenter;
        private ComboBox cmbPolygonForTest;
        private ComboBox cmbPolygonForTransform;

        public Form1()
        {
            InitializeComponent();
            SetupUI();
            UpdateControlsVisibility();
            UpdatePolygonSelectionComboBoxes();
        }

        private void SetupUI()
        {
            this.Text = "Аффинные преобразования";
            this.Size = new Size(1000, 700);
            this.StartPosition = FormStartPosition.CenterScreen;

            // Панель для рисования
            drawingPanel = new Panel
            {
                Location = new Point(10, 10),
                Size = new Size(700, 650),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            drawingPanel.MouseClick += DrawingPanel_MouseClick;
            drawingPanel.Paint += DrawingPanel_Paint;
            this.Controls.Add(drawingPanel);

            // Группа преобразований
            GroupBox grpTransform = new GroupBox
            {
                Text = "Аффинные преобразования",
                Location = new Point(720, 10),
                Size = new Size(260, 450)
            };

            // Выбор полигона для преобразований
            Label lblPolygonTransform = new Label
            {
                Text = "Полигон для преобразований:",
                Location = new Point(10, 20),
                Size = new Size(240, 15)
            };
            grpTransform.Controls.Add(lblPolygonTransform);

            cmbPolygonForTransform = new ComboBox
            {
                Location = new Point(10, 40),
                Size = new Size(240, 25),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbPolygonForTransform.SelectedIndexChanged += CmbPolygonForTransform_SelectedIndexChanged;
            grpTransform.Controls.Add(cmbPolygonForTransform);

            cmbTransformType = new ComboBox
            {
                Location = new Point(10, 75),
                Size = new Size(240, 25),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbTransformType.Items.AddRange(new string[] {
                "Смещение",
                "Поворот вокруг точки",
                "Поворот вокруг центра",
                "Масштабирование относительно точки",
                "Масштабирование относительно центра"
            });
            cmbTransformType.SelectedIndex = 0;
            cmbTransformType.SelectedIndexChanged += CmbTransformType_SelectedIndexChanged;
            grpTransform.Controls.Add(cmbTransformType);

            // Группа для параметров преобразования
            grpTransformParams = new GroupBox
            {
                Text = "Параметры преобразования",
                Location = new Point(10, 110),
                Size = new Size(240, 200)
            };

            // Параметры смещения
            lblDx = new Label { Text = "dx:", Location = new Point(10, 25), Size = new Size(25, 20) };
            txtDx = new TextBox { Location = new Point(35, 20), Size = new Size(50, 20), Text = "10" };
            lblDy = new Label { Text = "dy:", Location = new Point(95, 25), Size = new Size(25, 20) };
            txtDy = new TextBox { Location = new Point(120, 20), Size = new Size(50, 20), Text = "10" };

            // Параметры поворота
            lblAngle = new Label { Text = "Угол (°):", Location = new Point(10, 25), Size = new Size(50, 20) };
            txtAngle = new TextBox { Location = new Point(65, 20), Size = new Size(50, 20), Text = "45" };

            // Параметры масштабирования
            lblScaleX = new Label { Text = "Scale X:", Location = new Point(10, 25), Size = new Size(50, 20) };
            txtScaleX = new TextBox { Location = new Point(65, 20), Size = new Size(50, 20), Text = "1,5" };
            lblScaleY = new Label { Text = "Scale Y:", Location = new Point(125, 25), Size = new Size(50, 20) };
            txtScaleY = new TextBox { Location = new Point(180, 20), Size = new Size(50, 20), Text = "1,5" };

            // Кнопки для установки центров
            btnSetRotationCenter = new Button
            {
                Text = "Установить центр поворота",
                Location = new Point(10, 55),
                Size = new Size(220, 25)
            };
            btnSetRotationCenter.Click += (s, e) =>
            {
                isSettingRotationCenter = true;
                lblStatus.Text = "Кликните на панели для установки центра поворота";
            };

            btnSetScalingCenter = new Button
            {
                Text = "Установить центр масштабирования",
                Location = new Point(10, 85),
                Size = new Size(220, 25)
            };
            btnSetScalingCenter.Click += (s, e) =>
            {
                isSettingScalingCenter = true;
                lblStatus.Text = "Кликните на панели для установки центра масштабирования";
            };

            grpTransformParams.Controls.AddRange(new Control[] {
                lblDx, txtDx, lblDy, txtDy,
                lblAngle, txtAngle,
                lblScaleX, txtScaleX, lblScaleY, txtScaleY,
                btnSetRotationCenter, btnSetScalingCenter
            });

            grpTransform.Controls.Add(grpTransformParams);

            btnApplyTransform = new Button
            {
                Text = "Применить преобразование",
                Location = new Point(10, 320),
                Size = new Size(240, 30)
            };
            btnApplyTransform.Click += BtnApplyTransform_Click;
            grpTransform.Controls.Add(btnApplyTransform);

            this.Controls.Add(grpTransform);

            // Группа операций
            GroupBox grpOperations = new GroupBox
            {
                Text = "Операции",
                Location = new Point(720, 470),
                Size = new Size(260, 230) // Увеличили высоту для дополнительной кнопки
            };

            // Выбор полигона для проверки
            Label lblPolygonSelect = new Label
            {
                Text = "Полигон для проверки точки:",
                Location = new Point(10, 20),
                Size = new Size(240, 15)
            };
            grpOperations.Controls.Add(lblPolygonSelect);

            cmbPolygonForTest = new ComboBox
            {
                Location = new Point(10, 40),
                Size = new Size(240, 25),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbPolygonForTest.SelectedIndexChanged += CmbPolygonForTest_SelectedIndexChanged;
            grpOperations.Controls.Add(cmbPolygonForTest);

            btnClear = new Button
            {
                Text = "Очистить сцену",
                Location = new Point(10, 75),
                Size = new Size(240, 30)
            };
            btnClear.Click += BtnClear_Click;
            grpOperations.Controls.Add(btnClear);

            btnCheckPoint = new Button
            {
                Text = "Проверить точку в полигоне",
                Location = new Point(10, 115),
                Size = new Size(240, 30)
            };
            btnCheckPoint.Click += BtnCheckPoint_Click;
            grpOperations.Controls.Add(btnCheckPoint);

            btnCheckEdges = new Button
            {
                Text = "Проверить пересечение ребер",
                Location = new Point(10, 155),
                Size = new Size(240, 30)
            };
            btnCheckEdges.Click += BtnCheckEdges_Click;
            grpOperations.Controls.Add(btnCheckEdges);

            // Кнопка классификации точки - теперь внутри группы операций
            btnClassifyPoint = new Button
            {
                Text = "Классифицировать точку относительно ребра",
                Location = new Point(10, 195),
                Size = new Size(240, 30)
            };
            btnClassifyPoint.Click += BtnClassifyPoint_Click;
            grpOperations.Controls.Add(btnClassifyPoint);

            this.Controls.Add(grpOperations);

            // Статусная строка
            lblStatus = new Label
            {
                Text = "Кликните для создания полигона",
                Location = new Point(10, 665),
                Size = new Size(700, 20),
                BorderStyle = BorderStyle.FixedSingle
            };
            this.Controls.Add(lblStatus);

            rotationCenter = new PointF(350, 325);
            scalingCenter = new PointF(350, 325);
        }

        private void CmbTransformType_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateControlsVisibility();
        }

        private void CmbPolygonForTest_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbPolygonForTest.SelectedIndex >= 0 && cmbPolygonForTest.SelectedIndex < polygons.Count)
            {
                selectedPolygonForTest = polygons[cmbPolygonForTest.SelectedIndex];
                drawingPanel.Invalidate();
            }
            else
            {
                selectedPolygonForTest = null;
            }
        }

        private void CmbPolygonForTransform_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbPolygonForTransform.SelectedIndex >= 0 && cmbPolygonForTransform.SelectedIndex < polygons.Count)
            {
                selectedPolygonForTransform = polygons[cmbPolygonForTransform.SelectedIndex];
                drawingPanel.Invalidate();
            }
            else
            {
                selectedPolygonForTransform = null;
            }
        }

        private void UpdatePolygonSelectionComboBoxes()
        {
            cmbPolygonForTest.Items.Clear();
            cmbPolygonForTransform.Items.Clear();

            for (int i = 0; i < polygons.Count; i++)
            {
                string polygonType = GetPolygonType(polygons[i]);
                string itemText = $"Полигон {i + 1} ({polygonType}, {polygons[i].Points.Count} вершин)";
                cmbPolygonForTest.Items.Add(itemText);
                cmbPolygonForTransform.Items.Add(itemText);
            }

            if (polygons.Count > 0)
            {
                cmbPolygonForTest.SelectedIndex = 0;
                cmbPolygonForTransform.SelectedIndex = 0;
            }
            else
            {
                selectedPolygonForTest = null;
                selectedPolygonForTransform = null;
            }

            drawingPanel.Invalidate();
        }

        private void UpdateControlsVisibility()
        {
            // Сначала скрываем все элементы
            foreach (Control control in grpTransformParams.Controls)
            {
                control.Visible = false;
            }

            // Показываем нужные элементы в зависимости от выбранного преобразования
            switch (cmbTransformType.SelectedIndex)
            {
                case 0: // Смещение
                    grpTransformParams.Text = "Параметры смещения";
                    lblDx.Visible = true;
                    txtDx.Visible = true;
                    lblDy.Visible = true;
                    txtDy.Visible = true;
                    break;

                case 1: // Поворот вокруг точки
                    grpTransformParams.Text = "Параметры поворота вокруг точки";
                    lblAngle.Visible = true;
                    txtAngle.Visible = true;
                    btnSetRotationCenter.Visible = true;
                    break;

                case 2: // Поворот вокруг центра
                    grpTransformParams.Text = "Параметры поворота вокруг центра";
                    lblAngle.Visible = true;
                    txtAngle.Visible = true;
                    break;

                case 3: // Масштабирование относительно точки
                    grpTransformParams.Text = "Параметры масштабирования относительно точки";
                    lblScaleX.Visible = true;
                    txtScaleX.Visible = true;
                    lblScaleY.Visible = true;
                    txtScaleY.Visible = true;
                    btnSetScalingCenter.Visible = true;
                    break;

                case 4: // Масштабирование относительно центра
                    grpTransformParams.Text = "Параметры масштабирования относительно центра";
                    lblScaleX.Visible = true;
                    txtScaleX.Visible = true;
                    lblScaleY.Visible = true;
                    txtScaleY.Visible = true;
                    break;
            }
        }

        private void DrawingPanel_MouseClick(object sender, MouseEventArgs e)
        {
            PointF clickPoint = new PointF(e.X, e.Y);

            // Обработка установки центров преобразований
            if (isSettingRotationCenter)
            {
                rotationCenter = clickPoint;
                isSettingRotationCenter = false;
                lblStatus.Text = $"Центр поворота установлен в ({clickPoint.X:F1}, {clickPoint.Y:F1})";
                drawingPanel.Invalidate();
                return;
            }

            if (isSettingScalingCenter)
            {
                scalingCenter = clickPoint;
                isSettingScalingCenter = false;
                lblStatus.Text = $"Центр масштабирования установлен в ({clickPoint.X:F1}, {clickPoint.Y:F1})";
                drawingPanel.Invalidate();
                return;
            }

            // ВСЕГДА разрешаем создание полигонов левой кнопкой мыши, если не в специальном режиме
            if (e.Button == MouseButtons.Left && currentMode == InteractionMode.CreatePolygon)
            {
                HandlePolygonCreation(clickPoint, e.Button);
                drawingPanel.Invalidate();
                return;
            }

            // Обработка разных режимов работы
            switch (currentMode)
            {
                case InteractionMode.CreatePolygon:
                    HandlePolygonCreation(clickPoint, e.Button);
                    break;

                case InteractionMode.SetTestPoint:
                    HandleTestPointSetting(clickPoint);
                    break;

                case InteractionMode.SetEdge1:
                    HandleEdge1Setting(clickPoint);
                    break;

                case InteractionMode.SetEdge2:
                    HandleEdge2Setting(clickPoint);
                    break;

                case InteractionMode.SetEdgeForClassification:
                    HandleEdgeForClassificationSetting(clickPoint);
                    break;

                case InteractionMode.SetPointForClassification:
                    HandlePointForClassificationSetting(clickPoint);
                    break;
            }

            drawingPanel.Invalidate();
        }

        // Обработка создания полигонов
        private void HandlePolygonCreation(PointF clickPoint, MouseButtons button)
        {
            // Всегда разрешаем создание полигонов, независимо от текущего режима
            // (если только мы не в процессе установки центров)
            if (isSettingRotationCenter || isSettingScalingCenter)
                return;

            if (button == MouseButtons.Left)
            {
                if (currentPolygon == null)
                {
                    currentPolygon = new Polygon();
                    polygons.Add(currentPolygon);
                }
                currentPolygon.AddPoint(clickPoint);
                lblStatus.Text = $"Добавлена точка ({clickPoint.X:F1}, {clickPoint.Y:F1}). Полигон: {currentPolygon.Points.Count} вершин";
            }
            else if (button == MouseButtons.Right)
            {
                if (currentPolygon != null && currentPolygon.Points.Count > 0)
                {
                    currentPolygon.IsClosed = true;

                    // Определяем тип полигона для отображения в статусе
                    string polygonType = GetPolygonType(currentPolygon);

                    lblStatus.Text = $"Полигон завершен. Тип: {polygonType}, вершин: {currentPolygon.Points.Count}. Кликните для создания нового полигона";

                    // Обновляем комбобоксы выбора полигонов
                    UpdatePolygonSelectionComboBoxes();

                    currentPolygon = null;
                }
            }
        }

        // Обработка установки тестовой точки
        private void HandleTestPointSetting(PointF clickPoint)
        {
            testPoint = clickPoint;
            drawingPanel.Invalidate(); // Сразу перерисовываем, чтобы показать точку

            // Проверяем точку в выбранном полигоне
            CheckPointInPolygon();
        }

        // Обработка установки первого ребра
        private void HandleEdge1Setting(PointF clickPoint)
        {
            Edge? nearestEdge = FindNearestEdge(clickPoint);
            if (nearestEdge.HasValue)
            {
                testEdge1 = nearestEdge.Value;
                currentMode = InteractionMode.SetEdge2;
                lblStatus.Text = "Выберите второе ребро (кликните как можно ближе)";
                drawingPanel.Invalidate(); // Перерисовываем чтобы показать первое ребро
            }
            else
            {
                MessageBox.Show("Ребро не найдено. Кликните ближе к существующему ребру.", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        // Обработка установки второго ребра
        private void HandleEdge2Setting(PointF clickPoint)
        {
            Edge? nearestEdge = FindNearestEdge(clickPoint);
            if (nearestEdge.HasValue)
            {
                testEdge2 = nearestEdge.Value;
                drawingPanel.Invalidate(); // Сразу перерисовываем, чтобы показать оба ребра
                CheckEdgeIntersection();
            }
            else
            {
                MessageBox.Show("Ребро не найдено. Кликните ближе к существующему ребру.", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        // Обработка установки ребра для классификации
        private void HandleEdgeForClassificationSetting(PointF clickPoint)
        {
            Edge? nearestEdge = FindNearestEdge(clickPoint);
            if (nearestEdge.HasValue)
            {
                classificationEdge = nearestEdge.Value;
                currentMode = InteractionMode.SetPointForClassification;
                lblStatus.Text = "Выберите точку для классификации (кликните в нужном месте)";
                drawingPanel.Invalidate(); // Перерисовываем чтобы показать ребро
            }
            else
            {
                MessageBox.Show("Ребро не найдено. Кликните ближе к существующему ребру.", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        // Обработка установки точки для классификации
        private void HandlePointForClassificationSetting(PointF clickPoint)
        {
            classificationPoint = clickPoint;
            drawingPanel.Invalidate(); // Сразу перерисовываем, чтобы показать точку
            ClassifyPointRelativeToEdge();
        }

        // Поиск ближайшего ребра к точке
        private Edge? FindNearestEdge(PointF point, float maxDistance = 15.0f)
        {
            Edge? nearestEdge = null;
            float minDistance = float.MaxValue;

            foreach (var polygon in polygons)
            {
                for (int i = 0; i < polygon.Points.Count; i++)
                {
                    int nextIndex = (i + 1) % polygon.Points.Count;
                    if (!polygon.IsClosed && i == polygon.Points.Count - 1)
                        continue;

                    PointF start = polygon.Points[i];
                    PointF end = polygon.Points[nextIndex];
                    Edge edge = new Edge(start, end);

                    float distance = DistanceFromPointToEdge(point, edge);
                    if (distance < minDistance && distance <= maxDistance)
                    {
                        minDistance = distance;
                        nearestEdge = edge;
                    }
                }
            }

            return nearestEdge;
        }

        // Расстояние от точки до ребра
        private float DistanceFromPointToEdge(PointF point, Edge edge)
        {
            return PointToLineDistance(point, edge.Start, edge.End);
        }

        // Расстояние от точки до прямой
        private float PointToLineDistance(PointF point, PointF lineStart, PointF lineEnd)
        {
            float A = point.X - lineStart.X;
            float B = point.Y - lineStart.Y;
            float C = lineEnd.X - lineStart.X;
            float D = lineEnd.Y - lineStart.Y;

            float dot = A * C + B * D;
            float lenSq = C * C + D * D;
            float param = (lenSq != 0) ? dot / lenSq : -1;

            float xx, yy;

            if (param < 0)
            {
                xx = lineStart.X;
                yy = lineStart.Y;
            }
            else if (param > 1)
            {
                xx = lineEnd.X;
                yy = lineEnd.Y;
            }
            else
            {
                xx = lineStart.X + param * C;
                yy = lineStart.Y + param * D;
            }

            float dx = point.X - xx;
            float dy = point.Y - yy;
            return (float)Math.Sqrt(dx * dx + dy * dy);
        }

        // Определение типа полигона
        private string GetPolygonType(Polygon polygon)
        {
            if (polygon.Points.Count == 1)
                return "точка";
            else if (polygon.Points.Count == 2)
                return "отрезок";
            else if (polygon.IsConvex())
                return "выпуклый";
            else
                return "невыпуклый";
        }

        private void DrawingPanel_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // Рисуем полигоны
            for (int i = 0; i < polygons.Count; i++)
            {
                var polygon = polygons[i];

                // Выделяем выбранные полигоны
                if (polygon == selectedPolygonForTest)
                {
                    polygon.Draw(g, Color.Blue, Color.DarkBlue);
                }
                else if (polygon == selectedPolygonForTransform)
                {
                    polygon.Draw(g, Color.Green, Color.DarkGreen);
                }
                else
                {
                    polygon.Draw(g, Color.Black, Color.Gray);
                }

                // Подписываем полигоны номерами
                if (polygon.Points.Count > 0)
                {
                    PointF center = polygon.GetCenter();
                    string label = $"#{i + 1}";

                    // Проверяем статусы полигона
                    bool isTest = polygon == selectedPolygonForTest;
                    bool isTransform = polygon == selectedPolygonForTransform;

                    // Формируем строку статусов
                    if (isTest && isTransform)
                        label += " (тест, преобр)";
                    else if (isTest)
                        label += " (тест)";
                    else if (isTransform)
                        label += " (преобр)";

                    g.DrawString(label, this.Font, Brushes.Black, center.X + 5, center.Y - 10);
                }
            }

            // Рисуем тестовую точку (всегда, если она установлена)
            if (testPoint.HasValue)
            {
                g.FillEllipse(Brushes.Red, testPoint.Value.X - 3, testPoint.Value.Y - 3, 6, 6);
                g.DrawEllipse(Pens.DarkRed, testPoint.Value.X - 5, testPoint.Value.Y - 5, 10, 10);
                g.DrawString("Выбранная точка", this.Font, Brushes.Red, testPoint.Value.X + 5, testPoint.Value.Y - 6);
            }

            // Рисуем тестовые ребра (всегда, если они установлены)
            if (testEdge1.HasValue)
            {
                DrawEdge(g, testEdge1.Value, Pens.Blue, "Ребро 1");
            }
            if (testEdge2.HasValue)
            {
                DrawEdge(g, testEdge2.Value, Pens.Green, "Ребро 2");
            }

            // Рисуем ребро и точку для классификации (всегда, если они установлены)
            if (classificationEdge.HasValue)
            {
                DrawEdge(g, classificationEdge.Value, Pens.Orange, "Ребро для классификации");

                // Рисуем стрелку направления
                DrawDirectionArrow(g, classificationEdge.Value);
            }
            if (classificationPoint.HasValue)
            {
                g.FillEllipse(Brushes.Purple, classificationPoint.Value.X - 3, classificationPoint.Value.Y - 3, 6, 6);
                g.DrawEllipse(Pens.DarkViolet, classificationPoint.Value.X - 5, classificationPoint.Value.Y - 5, 10, 10);
                g.DrawString("Точка для классификации", this.Font, Brushes.Purple, classificationPoint.Value.X + 5, classificationPoint.Value.Y - 6);
            }

            // Рисуем центры преобразований
            g.FillEllipse(Brushes.Orange, rotationCenter.X - 4, rotationCenter.Y - 4, 8, 8);
            g.DrawString("R", this.Font, Brushes.Orange, rotationCenter.X + 5, rotationCenter.Y - 6);

            g.FillEllipse(Brushes.Purple, scalingCenter.X - 4, scalingCenter.Y - 4, 8, 8);
            g.DrawString("S", this.Font, Brushes.Purple, scalingCenter.X + 5, scalingCenter.Y - 6);
        }

        private void DrawEdge(Graphics g, Edge edge, Pen pen, string label = "")
        {
            g.DrawLine(pen, edge.Start, edge.End);
            g.FillEllipse(Brushes.Blue, edge.Start.X - 2, edge.Start.Y - 2, 4, 4);
            g.FillEllipse(Brushes.Green, edge.End.X - 2, edge.End.Y - 2, 4, 4);

            if (!string.IsNullOrEmpty(label))
            {
                PointF midPoint = new PointF((edge.Start.X + edge.End.X) / 2, (edge.Start.Y + edge.End.Y) / 2);
                g.DrawString(label, this.Font, pen.Brush, midPoint.X + 5, midPoint.Y - 6);
            }
        }

        // Функция: Рисование стрелки направления
        private void DrawDirectionArrow(Graphics g, Edge edge)
        {
            // Вычисляем середину ребра
            PointF midPoint = new PointF(
                (edge.Start.X + edge.End.X) / 2,
                (edge.Start.Y + edge.End.Y) / 2
            );

            // Вычисляем направление ребра
            float dx = edge.End.X - edge.Start.X;
            float dy = edge.End.Y - edge.Start.Y;

            // Нормализуем вектор направления
            float length = (float)Math.Sqrt(dx * dx + dy * dy);
            if (length > 0)
            {
                dx /= length;
                dy /= length;
            }

            // Вычисляем перпендикуляр для стрелки
            float arrowLength = 10;
            float arrowWidth = 6;

            // Точки стрелки
            PointF arrowPoint1 = new PointF(
                midPoint.X - dx * arrowLength - dy * arrowWidth,
                midPoint.Y - dy * arrowLength + dx * arrowWidth
            );

            PointF arrowPoint2 = new PointF(
                midPoint.X - dx * arrowLength + dy * arrowWidth,
                midPoint.Y - dy * arrowLength - dx * arrowWidth
            );

            // Рисуем стрелку
            g.FillPolygon(Brushes.Red, new PointF[] { midPoint, arrowPoint1, arrowPoint2 });
        }

        private void BtnApplyTransform_Click(object sender, EventArgs e)
        {
            if (selectedPolygonForTransform == null)
            {
                MessageBox.Show("Выберите полигон для преобразования", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            Polygon polygon = selectedPolygonForTransform;
            Matrix3x3 transformMatrix = Matrix3x3.Identity;

            try
            {
                switch (cmbTransformType.SelectedIndex)
                {
                    case 0: // Смещение
                        float dx = float.Parse(txtDx.Text);
                        float dy = float.Parse(txtDy.Text);
                        transformMatrix = AffineTransformations.Translation(dx, dy);
                        break;

                    case 1: // Поворот вокруг точки
                        float angle1 = float.Parse(txtAngle.Text);
                        transformMatrix = AffineTransformations.RotationAroundPoint(angle1, rotationCenter);
                        break;

                    case 2: // Поворот вокруг центра
                        float angle2 = float.Parse(txtAngle.Text);
                        PointF center = polygon.GetCenter();
                        transformMatrix = AffineTransformations.RotationAroundPoint(angle2, center);
                        break;

                    case 3: // Масштабирование относительно точки
                        float scaleX1 = float.Parse(txtScaleX.Text);
                        float scaleY1 = float.Parse(txtScaleY.Text);
                        transformMatrix = AffineTransformations.ScalingAroundPoint(scaleX1, scaleY1, scalingCenter);
                        break;

                    case 4: // Масштабирование относительно центра
                        float scaleX2 = float.Parse(txtScaleX.Text);
                        float scaleY2 = float.Parse(txtScaleY.Text);
                        PointF polyCenter = polygon.GetCenter();
                        transformMatrix = AffineTransformations.ScalingAroundPoint(scaleX2, scaleY2, polyCenter);
                        break;
                }

                polygon.Transform(transformMatrix);
                drawingPanel.Invalidate();
                lblStatus.Text = "Преобразование применено";
            }
            catch (FormatException)
            {
                MessageBox.Show("Пожалуйста, введите корректные числовые значения", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnClear_Click(object sender, EventArgs e)
        {
            polygons.Clear();
            currentPolygon = null;
            testPoint = null;
            testEdge1 = null;
            testEdge2 = null;
            classificationEdge = null;
            classificationPoint = null;
            selectedPolygonForTest = null;
            selectedPolygonForTransform = null;

            // ВАЖНОЕ ИСПРАВЛЕНИЕ: сбрасываем режим на создание полигонов
            currentMode = InteractionMode.CreatePolygon;
            isSettingRotationCenter = false;
            isSettingScalingCenter = false;

            // Обновляем комбобоксы
            UpdatePolygonSelectionComboBoxes();

            drawingPanel.Invalidate();
            lblStatus.Text = "Сцена очищена. Кликните для создания нового полигона";
        }

        private void BtnCheckPoint_Click(object sender, EventArgs e)
        {
            if (polygons.Count == 0)
            {
                MessageBox.Show("Сначала создайте полигоны", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Проверяем, выбран ли полигон в комбобоксе
            if (selectedPolygonForTest == null && polygons.Count > 0)
            {
                // Если не выбран, но есть полигоны - выбираем первый
                selectedPolygonForTest = polygons[0];
                cmbPolygonForTest.SelectedIndex = 0;
            }

            currentMode = InteractionMode.SetTestPoint;
            lblStatus.Text = "Кликните на панели для установки тестовой точки";
        }

        private void BtnCheckEdges_Click(object sender, EventArgs e)
        {
            if (polygons.Count == 0)
            {
                MessageBox.Show("Сначала создайте полигоны", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            currentMode = InteractionMode.SetEdge1;
            testEdge1 = null;
            testEdge2 = null;
            lblStatus.Text = "Выберите первое ребро (кликните как можно ближе)";
        }

        private void BtnClassifyPoint_Click(object sender, EventArgs e)
        {
            if (polygons.Count == 0)
            {
                MessageBox.Show("Сначала создайте полигоны", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            currentMode = InteractionMode.SetEdgeForClassification;
            classificationEdge = null;
            classificationPoint = null;
            lblStatus.Text = "Выберите ребро для классификации (кликните как можно ближе)";
        }

        private void CheckPointInPolygon()
        {
            if (!testPoint.HasValue || polygons.Count == 0)
            {
                MessageBox.Show("Нет тестовой точки или полигонов", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string result = "";

            if (selectedPolygonForTest != null)
            {
                // Проверяем относительно конкретного полигона
                if (selectedPolygonForTest.Points.Count < 3)
                {
                    MessageBox.Show("Выбранный полигон должен иметь хотя бы 3 вершины", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                bool isInside = IsPointInPolygon(testPoint.Value, selectedPolygonForTest.Points);
                string convexity = selectedPolygonForTest.IsConvex() ? "выпуклого" : "невыпуклого";
                int polygonIndex = polygons.IndexOf(selectedPolygonForTest) + 1;

                result = $"Точка {((isInside) ? "внутри" : "вне")} {convexity} полигона #{polygonIndex}";
            }
            else
            {
                // Проверяем относительно всех полигонов
                foreach (var polygon in polygons)
                {
                    if (polygon.Points.Count < 3) continue;

                    bool isInside = IsPointInPolygon(testPoint.Value, polygon.Points);
                    string convexity = polygon.IsConvex() ? "выпуклого" : "невыпуклого";
                    int polygonIndex = polygons.IndexOf(polygon) + 1;

                    result += $"Точка {((isInside) ? "внутри" : "вне")} {convexity} полигона #{polygonIndex}\n";
                }

                if (string.IsNullOrEmpty(result))
                {
                    result = "Нет подходящих полигонов для проверки (нужны полигоны с 3+ вершинами)";
                }
            }

            // Показываем результат, но НЕ сбрасываем точку и режим
            MessageBox.Show(result, "Результат проверки точки", MessageBoxButtons.OK, MessageBoxIcon.Information);

            // После закрытия диалога возвращаемся в обычный режим, но точка остается видимой
            currentMode = InteractionMode.CreatePolygon;
            lblStatus.Text = "Режим: создание полигонов. Точка остается для визуализации";
        }

        private void CheckEdgeIntersection()
        {
            if (!testEdge1.HasValue || !testEdge2.HasValue)
            {
                MessageBox.Show("Не выбраны оба ребра", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            bool intersects = DoEdgesIntersect(testEdge1.Value, testEdge2.Value);

            // Дополнительная информация о ребрах
            string edge1Info = $"Ребро 1: ({testEdge1.Value.Start.X:F1}, {testEdge1.Value.Start.Y:F1}) - ({testEdge1.Value.End.X:F1}, {testEdge1.Value.End.Y:F1})";
            string edge2Info = $"Ребро 2: ({testEdge2.Value.Start.X:F1}, {testEdge2.Value.Start.Y:F1}) - ({testEdge2.Value.End.X:F1}, {testEdge2.Value.End.Y:F1})";

            // НЕ сбрасываем ребра - они остаются выделенными до закрытия окна
            MessageBox.Show($"{edge1Info}\n{edge2Info}\n\nРебра {((intersects) ? "пересекаются" : "не пересекаются")}",
                          "Результат проверки пересечения", MessageBoxButtons.OK, MessageBoxIcon.Information);

            // После закрытия диалога сбрасываем ребра и возвращаемся в обычный режим
            testEdge1 = null;
            testEdge2 = null;
            currentMode = InteractionMode.CreatePolygon;
            lblStatus.Text = "Режим: создание полигонов";
            drawingPanel.Invalidate();
        }

        private void ClassifyPointRelativeToEdge()
        {
            if (!classificationEdge.HasValue || !classificationPoint.HasValue)
            {
                MessageBox.Show("Не выбраны ребро или точка", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            Edge edge = classificationEdge.Value;
            PointF point = classificationPoint.Value;

            // ВЫЧИСЛЯЕМ ОТНОСИТЕЛЬНО НАПРАВЛЕНИЯ РЕБРА
            // Используем исходное направление ребра (от Start к End)
            PointF start = edge.Start;
            PointF end = edge.End;

            float cross = CrossProduct(start, end, point);

            string position;
            if (Math.Abs(cross) < 0.001f)
            {
                position = "НА РЕБРЕ";
            }
            else
            {
                // Определяем положение только как СЛЕВА или СПРАВА
                // В системе координат с осью Y вниз:
                // - Если cross > 0, точка справа от направления ребра
                // - Если cross < 0, точка слева от направления ребра
                position = cross > 0 ? "СПРАВА" : "СЛЕВА";
            }

            // НЕ сбрасываем сразу - показываем диалог с выделенными элементами
            MessageBox.Show($"Точка находится {position} от ребра\n" +
                          $"Векторное произведение: {cross:F2}\n" +
                          $"Ребро: ({edge.Start.X:F1}, {edge.Start.Y:F1}) - ({edge.End.X:F1}, {edge.End.Y:F1})",
                          "Классификация точки", MessageBoxButtons.OK, MessageBoxIcon.Information);

            // После закрытия диалога сбрасываем выделение
            classificationEdge = null;
            classificationPoint = null;
            currentMode = InteractionMode.CreatePolygon;
            lblStatus.Text = "Режим: создание полигонов";
            drawingPanel.Invalidate();
        }

        // Алгоритм проверки принадлежности точки полигону (метод лучей)
        private bool IsPointInPolygon(PointF point, List<PointF> polygon)
        {
            int count = 0;
            for (int i = 0; i < polygon.Count; i++)
            {
                PointF p1 = polygon[i];
                PointF p2 = polygon[(i + 1) % polygon.Count];

                if (point.Y > Math.Min(p1.Y, p2.Y) && point.Y <= Math.Max(p1.Y, p2.Y) &&
                    point.X <= Math.Max(p1.X, p2.X) && p1.Y != p2.Y)
                {
                    float xinters = (point.Y - p1.Y) * (p2.X - p1.X) / (p2.Y - p1.Y) + p1.X;
                    if (p1.X == p2.X || point.X <= xinters)
                        count++;
                }
            }
            return count % 2 != 0;
        }

        // Алгоритм проверки пересечения отрезков
        private bool DoEdgesIntersect(Edge e1, Edge e2)
        {
            float CrossProduct(PointF a, PointF b, PointF c)
            {
                return (b.X - a.X) * (c.Y - a.Y) - (b.Y - a.Y) * (c.X - a.X);
            }

            bool IsPointOnSegment(PointF p, PointF a, PointF b)
            {
                float cross = CrossProduct(a, b, p);
                if (Math.Abs(cross) > float.Epsilon)
                    return false;

                return p.X >= Math.Min(a.X, b.X) && p.X <= Math.Max(a.X, b.X) &&
                       p.Y >= Math.Min(a.Y, b.Y) && p.Y <= Math.Max(a.Y, b.Y);
            }

            bool PointsEqual(PointF a, PointF b, float tolerance = 0.0001f)
            {
                return Math.Abs(a.X - b.X) < tolerance && Math.Abs(a.Y - b.Y) < tolerance;
            }

            float cp1 = CrossProduct(e1.Start, e1.End, e2.Start);
            float cp2 = CrossProduct(e1.Start, e1.End, e2.End);
            float cp3 = CrossProduct(e2.Start, e2.End, e1.Start);
            float cp4 = CrossProduct(e2.Start, e2.End, e1.End);

            // СЛУЧАЙ 1: Общие конечные точки
            bool shareEndpoint =
                PointsEqual(e1.Start, e2.Start) || PointsEqual(e1.Start, e2.End) ||
                PointsEqual(e1.End, e2.Start) || PointsEqual(e1.End, e2.End);

            if (shareEndpoint)
                return true;

            // СЛУЧАЙ 2: Один из концов лежит на другом отрезке
            bool endpointOnSegment =
                IsPointOnSegment(e1.Start, e2.Start, e2.End) ||
                IsPointOnSegment(e1.End, e2.Start, e2.End) ||
                IsPointOnSegment(e2.Start, e1.Start, e1.End) ||
                IsPointOnSegment(e2.End, e1.Start, e1.End);

            if (endpointOnSegment)
                return true;

            // СЛУЧАЙ 3: Классическое пересечение
            bool properIntersection = (cp1 * cp2 < 0) && (cp3 * cp4 < 0);

            if (properIntersection)
                return true;

            return false;
        }

        private float CrossProduct(PointF a, PointF b, PointF c)
        {
            return (b.X - a.X) * (c.Y - a.Y) - (b.Y - a.Y) * (c.X - a.X);
        }
    }

    // Класс для представления полигона
    public class Polygon
    {
        public List<PointF> Points { get; private set; } = new List<PointF>();
        public bool IsClosed { get; set; } = false;

        public void AddPoint(PointF point)
        {
            Points.Add(point);
        }

        public void Transform(Matrix3x3 matrix)
        {
            for (int i = 0; i < Points.Count; i++)
            {
                Points[i] = matrix.Transform(Points[i]);
            }
        }

        public PointF GetCenter()
        {
            if (Points.Count == 0) return PointF.Empty;

            float sumX = 0, sumY = 0;
            foreach (var point in Points)
            {
                sumX += point.X;
                sumY += point.Y;
            }
            return new PointF(sumX / Points.Count, sumY / Points.Count);
        }

        public bool IsConvex()
        {
            if (Points.Count < 3) return true;

            bool sign = false;
            for (int i = 0; i < Points.Count; i++)
            {
                PointF p1 = Points[i];
                PointF p2 = Points[(i + 1) % Points.Count];
                PointF p3 = Points[(i + 2) % Points.Count];

                float cross = (p2.X - p1.X) * (p3.Y - p2.Y) - (p2.Y - p1.Y) * (p3.X - p2.X);
                if (i == 0)
                {
                    sign = cross > 0;
                }
                else if ((cross > 0) != sign)
                {
                    return false;
                }
            }
            return true;
        }

        public void Draw(Graphics g)
        {
            Draw(g, Color.Black, Color.Gray);
        }

        public void Draw(Graphics g, Color edgeColor, Color vertexColor)
        {
            if (Points.Count == 0) return;

            // Рисуем вершины
            foreach (var point in Points)
            {
                g.FillEllipse(new SolidBrush(vertexColor), point.X - 2, point.Y - 2, 4, 4);
            }

            // Рисуем ребра
            if (Points.Count > 1)
            {
                using (Pen pen = new Pen(edgeColor, 2))
                {
                    for (int i = 0; i < Points.Count - 1; i++)
                    {
                        g.DrawLine(pen, Points[i], Points[i + 1]);
                    }

                    if (IsClosed && Points.Count > 2)
                    {
                        g.DrawLine(pen, Points[Points.Count - 1], Points[0]);
                    }
                }
            }
        }
    }

    // Структура для представления ребра
    public struct Edge
    {
        public PointF Start { get; set; }
        public PointF End { get; set; }

        public Edge(PointF start, PointF end)
        {
            Start = start;
            End = end;
        }
    }

    // Класс для матричных преобразований 3x3
    public class Matrix3x3
    {
        private float[,] m = new float[3, 3];

        public static Matrix3x3 Identity => new Matrix3x3(
            1, 0, 0,
            0, 1, 0,
            0, 0, 1);

        public Matrix3x3(float m00, float m01, float m02,
                        float m10, float m11, float m12,
                        float m20, float m21, float m22)
        {
            m[0, 0] = m00; m[0, 1] = m01; m[0, 2] = m02;
            m[1, 0] = m10; m[1, 1] = m11; m[1, 2] = m12;
            m[2, 0] = m20; m[2, 1] = m21; m[2, 2] = m22;
        }

        public PointF Transform(PointF point)
        {
            float x = point.X * m[0, 0] + point.Y * m[1, 0] + m[2, 0];
            float y = point.X * m[0, 1] + point.Y * m[1, 1] + m[2, 1];
            float w = point.X * m[0, 2] + point.Y * m[1, 2] + m[2, 2];

            return new PointF(x / w, y / w);
        }

        public static Matrix3x3 operator *(Matrix3x3 a, Matrix3x3 b)
        {
            var result = new Matrix3x3(0, 0, 0, 0, 0, 0, 0, 0, 0);
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    for (int k = 0; k < 3; k++)
                    {
                        result.m[i, j] += a.m[i, k] * b.m[k, j];
                    }
                }
            }
            return result;
        }
    }

    // Статический класс с методами аффинных преобразований
    public static class AffineTransformations
    {
        public static Matrix3x3 Translation(float dx, float dy)
        {
            return new Matrix3x3(
                1, 0, 0,
                0, 1, 0,
                dx, dy, 1);
        }

        public static Matrix3x3 Rotation(float angle)
        {
            float rad = angle * (float)Math.PI / 180f;
            float cos = (float)Math.Cos(rad);
            float sin = (float)Math.Sin(rad);

            return new Matrix3x3(
                cos, sin, 0,
                -sin, cos, 0,
                0, 0, 1);
        }

        public static Matrix3x3 Scaling(float sx, float sy)
        {
            return new Matrix3x3(
                sx, 0, 0,
                0, sy, 0,
                0, 0, 1);
        }

        public static Matrix3x3 RotationAroundPoint(float angle, PointF center)
        {
            return Translation(-center.X, -center.Y) *
                   Rotation(angle) *
                   Translation(center.X, center.Y);
        }

        public static Matrix3x3 ScalingAroundPoint(float sx, float sy, PointF center)
        {
            return Translation(-center.X, -center.Y) *
                   Scaling(sx, sy) *
                   Translation(center.X, center.Y);
        }
    }
}