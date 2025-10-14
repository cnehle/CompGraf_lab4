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

        // Элементы управления
        private ComboBox cmbTransformType;
        private TextBox txtDx, txtDy, txtAngle, txtScaleX, txtScaleY;
        private Button btnApplyTransform, btnClear, btnCheckPoint, btnCheckEdges;
        private Label lblStatus;
        private Panel drawingPanel;
        private GroupBox grpTransformParams;
        private Label lblDx, lblDy, lblAngle, lblScaleX, lblScaleY;
        private Button btnSetRotationCenter, btnSetScalingCenter;

        public Form1()
        {
            InitializeComponent();
            SetupUI();
            UpdateControlsVisibility();
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
                Size = new Size(260, 400)
            };

            cmbTransformType = new ComboBox
            {
                Location = new Point(10, 20),
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
                Location = new Point(10, 50),
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
                Location = new Point(10, 55),
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
                Location = new Point(10, 260),
                Size = new Size(240, 30)
            };
            btnApplyTransform.Click += BtnApplyTransform_Click;
            grpTransform.Controls.Add(btnApplyTransform);

            this.Controls.Add(grpTransform);

            // Группа операций
            GroupBox grpOperations = new GroupBox
            {
                Text = "Операции",
                Location = new Point(720, 420),
                Size = new Size(260, 150)
            };

            btnClear = new Button
            {
                Text = "Очистить сцену",
                Location = new Point(10, 20),
                Size = new Size(240, 30)
            };
            btnClear.Click += BtnClear_Click;
            grpOperations.Controls.Add(btnClear);

            btnCheckPoint = new Button
            {
                Text = "Проверить точку в полигоне",
                Location = new Point(10, 60),
                Size = new Size(240, 30)
            };
            btnCheckPoint.Click += BtnCheckPoint_Click;
            grpOperations.Controls.Add(btnCheckPoint);

            btnCheckEdges = new Button
            {
                Text = "Проверить пересечение ребер",
                Location = new Point(10, 100),
                Size = new Size(240, 30)
            };
            btnCheckEdges.Click += BtnCheckEdges_Click;
            grpOperations.Controls.Add(btnCheckEdges);

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

            if (e.Button == MouseButtons.Left)
            {
                if (currentPolygon == null)
                {
                    currentPolygon = new Polygon();
                    polygons.Add(currentPolygon);
                }
                currentPolygon.AddPoint(clickPoint);
                lblStatus.Text = $"Добавлена точка ({clickPoint.X:F1}, {clickPoint.Y:F1}). Полигон: {currentPolygon.Points.Count} вершин";
            }
            else if (e.Button == MouseButtons.Right)
            {
                if (currentPolygon != null && currentPolygon.Points.Count > 0)
                {
                    currentPolygon.IsClosed = true;

                    // Определяем тип полигона для отображения в статусе
                    string polygonType = "неопределенный";
                    if (currentPolygon.Points.Count == 1)
                        polygonType = "точка";
                    else if (currentPolygon.Points.Count == 2)
                        polygonType = "отрезок";
                    else if (currentPolygon.IsConvex())
                        polygonType = "выпуклый";
                    else
                        polygonType = "невыпуклый";

                    lblStatus.Text = $"Полигон завершен. Тип: {polygonType}, вершин: {currentPolygon.Points.Count}. Кликните для создания нового полигона";
                    currentPolygon = null;
                }
            }

            drawingPanel.Invalidate();
        }

        private void DrawingPanel_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // Рисуем полигоны
            foreach (var polygon in polygons)
            {
                polygon.Draw(g);
            }

            // Рисуем тестовую точку
            if (testPoint.HasValue)
            {
                g.FillEllipse(Brushes.Red, testPoint.Value.X - 3, testPoint.Value.Y - 3, 6, 6);
            }

            // Рисуем тестовые ребра
            if (testEdge1.HasValue)
            {
                DrawEdge(g, testEdge1.Value, Pens.Blue);
            }
            if (testEdge2.HasValue)
            {
                DrawEdge(g, testEdge2.Value, Pens.Green);
            }

            // Рисуем центры преобразований
            g.FillEllipse(Brushes.Orange, rotationCenter.X - 4, rotationCenter.Y - 4, 8, 8);
            g.DrawString("R", this.Font, Brushes.Orange, rotationCenter.X + 5, rotationCenter.Y - 6);

            g.FillEllipse(Brushes.Purple, scalingCenter.X - 4, scalingCenter.Y - 4, 8, 8);
            g.DrawString("S", this.Font, Brushes.Purple, scalingCenter.X + 5, scalingCenter.Y - 6);
        }

        private void DrawEdge(Graphics g, Edge edge, Pen pen)
        {
            g.DrawLine(pen, edge.Start, edge.End);
            g.FillEllipse(Brushes.Blue, edge.Start.X - 2, edge.Start.Y - 2, 4, 4);
            g.FillEllipse(Brushes.Green, edge.End.X - 2, edge.End.Y - 2, 4, 4);
        }

        private void BtnApplyTransform_Click(object sender, EventArgs e)
        {
            if (polygons.Count == 0 || polygons.Last().Points.Count == 0)
            {
                MessageBox.Show("Создайте полигон для применения преобразования");
                return;
            }

            Polygon polygon = polygons.Last();
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
                MessageBox.Show("Пожалуйста, введите корректные числовые значения");
            }
        }

        private void BtnClear_Click(object sender, EventArgs e)
        {
            polygons.Clear();
            currentPolygon = null;
            testPoint = null;
            testEdge1 = null;
            testEdge2 = null;
            drawingPanel.Invalidate();
            lblStatus.Text = "Сцена очищена";
        }

        private void BtnCheckPoint_Click(object sender, EventArgs e)
        {
            if (polygons.Count == 0)
            {
                MessageBox.Show("Создайте полигон для проверки");
                return;
            }

            lblStatus.Text = "Введите координаты точки для проверки";
            var result = GetPointFromUser();
            if (result.HasValue)
            {
                testPoint = result.Value;
                CheckPointInPolygon();
                drawingPanel.Invalidate();
            }
        }

        private void BtnCheckEdges_Click(object sender, EventArgs e)
        {
            lblStatus.Text = "Введите координаты первого ребра";
            testEdge1 = GetEdgeFromUser();
            if (testEdge1.HasValue)
            {
                lblStatus.Text = "Введите координаты второго ребра";
                testEdge2 = GetEdgeFromUser();
                if (testEdge2.HasValue)
                {
                    CheckEdgeIntersection();
                    drawingPanel.Invalidate();
                }
            }
        }

        private PointF? GetPointFromUser()
        {
            using (var form = new PointInputForm())
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    return form.Point;
                }
            }
            return null;
        }

        private Edge? GetEdgeFromUser()
        {
            using (var form = new EdgeInputForm())
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    return form.Edge;
                }
            }
            return null;
        }

        private void CheckPointInPolygon()
        {
            if (!testPoint.HasValue || polygons.Count == 0) return;

            foreach (var polygon in polygons)
            {
                if (polygon.Points.Count < 3) continue;

                bool isInside = IsPointInPolygon(testPoint.Value, polygon.Points);
                string convexity = polygon.IsConvex() ? "выпуклого" : "невыпуклого";

                MessageBox.Show($"Точка {((isInside) ? "внутри" : "вне")} {convexity} полигона");
            }
        }

        private void CheckEdgeIntersection()
        {
            if (!testEdge1.HasValue || !testEdge2.HasValue) return;

            bool intersects = DoEdgesIntersect(testEdge1.Value, testEdge2.Value);
            MessageBox.Show($"Ребра {((intersects) ? "пересекаются" : "не пересекаются")}");
        }

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

        private bool DoEdgesIntersect(Edge e1, Edge e2)
        {
            float CrossProduct(PointF a, PointF b, PointF c)
            {
                return (b.X - a.X) * (c.Y - a.Y) - (b.Y - a.Y) * (c.X - a.X);
            }

            float cp1 = CrossProduct(e1.Start, e1.End, e2.Start);
            float cp2 = CrossProduct(e1.Start, e1.End, e2.End);
            float cp3 = CrossProduct(e2.Start, e2.End, e1.Start);
            float cp4 = CrossProduct(e2.Start, e2.End, e1.End);

            return (cp1 * cp2 < 0) && (cp3 * cp4 < 0);
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
            if (Points.Count == 0) return;

            // Рисуем вершины
            foreach (var point in Points)
            {
                g.FillEllipse(Brushes.Black, point.X - 2, point.Y - 2, 4, 4);
            }

            // Рисуем ребра
            if (Points.Count > 1)
            {
                for (int i = 0; i < Points.Count - 1; i++)
                {
                    g.DrawLine(Pens.Black, Points[i], Points[i + 1]);
                }

                if (IsClosed && Points.Count > 2)
                {
                    g.DrawLine(Pens.Black, Points[Points.Count - 1], Points[0]);
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

    // Вспомогательные формы для ввода
    public class PointInputForm : Form
    {
        public PointF Point { get; private set; }
        private TextBox txtX, txtY;

        public PointInputForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Size = new Size(500, 250);
            this.Text = "Ввод координат точки";
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // Заголовок
            Label lblTitle = new Label
            {
                Text = "Введите координаты точки:",
                Location = new Point(10, 10),
                Size = new Size(280, 20),
                Font = new Font(this.Font, FontStyle.Bold)
            };

            // Координата X
            Label lblX = new Label { Text = "Координата X:", Location = new Point(20, 40), Size = new Size(100, 20) };
            txtX = new TextBox { Location = new Point(130, 40), Size = new Size(120, 25), Text = "100" };

            // Координата Y
            Label lblY = new Label { Text = "Координата Y:", Location = new Point(20, 70), Size = new Size(100, 20) };
            txtY = new TextBox { Location = new Point(130, 70), Size = new Size(120, 25), Text = "100" };

            // Кнопки
            Button btnOk = new Button { Text = "OK", Location = new Point(80, 100), Size = new Size(70, 30) };
            btnOk.Click += BtnOk_Click;

            Button btnCancel = new Button { Text = "Cancel", Location = new Point(160, 100), Size = new Size(70, 30) };
            btnCancel.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };

            this.Controls.AddRange(new Control[] {
                lblTitle, lblX, txtX, lblY, txtY, btnOk, btnCancel
            });

            // Устанавливаем фокус на первое поле ввода
            this.ActiveControl = txtX;
        }

        private void BtnOk_Click(object sender, EventArgs e)
        {
            if (float.TryParse(txtX.Text, out float x) && float.TryParse(txtY.Text, out float y))
            {
                Point = new PointF(x, y);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                MessageBox.Show("Введите корректные числовые координаты", "Ошибка ввода",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    public class EdgeInputForm : Form
    {
        public Edge Edge { get; private set; }
        private TextBox txtStartX, txtStartY, txtEndX, txtEndY;

        public EdgeInputForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Size = new Size(500, 400);
            this.Text = "Ввод координат ребра";
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // Заголовок
            Label lblTitle = new Label
            {
                Text = "Введите координаты начала и конца ребра:",
                Location = new Point(10, 10),
                Size = new Size(330, 20),
                Font = new Font(this.Font, FontStyle.Bold)
            };

            // Заголовок для начала ребра
            Label lblStartTitle = new Label
            {
                Text = "Начало ребра:",
                Location = new Point(20, 40),
                Size = new Size(150, 20),
                Font = new Font(this.Font, FontStyle.Underline)
            };

            // Координаты начала ребра
            Label lblStartX = new Label { Text = "X начала:", Location = new Point(30, 70), Size = new Size(70, 20) };
            txtStartX = new TextBox { Location = new Point(110, 70), Size = new Size(120, 25), Text = "100" };

            Label lblStartY = new Label { Text = "Y начала:", Location = new Point(30, 100), Size = new Size(70, 20) };
            txtStartY = new TextBox { Location = new Point(110, 100), Size = new Size(120, 25), Text = "100" };

            // Заголовок для конца ребра
            Label lblEndTitle = new Label
            {
                Text = "Конец ребра:",
                Location = new Point(20, 130),
                Size = new Size(150, 20),
                Font = new Font(this.Font, FontStyle.Underline)
            };

            // Координаты конца ребра
            Label lblEndX = new Label { Text = "X конца:", Location = new Point(30, 160), Size = new Size(70, 20) };
            txtEndX = new TextBox { Location = new Point(110, 160), Size = new Size(120, 25), Text = "200" };

            Label lblEndY = new Label { Text = "Y конца:", Location = new Point(30, 190), Size = new Size(70, 20) };
            txtEndY = new TextBox { Location = new Point(110, 190), Size = new Size(120, 25), Text = "200" };

            // Кнопки
            Button btnOk = new Button { Text = "OK", Location = new Point(100, 220), Size = new Size(70, 30) };
            btnOk.Click += BtnOk_Click;

            Button btnCancel = new Button { Text = "Cancel", Location = new Point(180, 220), Size = new Size(70, 30) };
            btnCancel.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };

            this.Controls.AddRange(new Control[] {
                lblTitle,
                lblStartTitle, lblStartX, txtStartX, lblStartY, txtStartY,
                lblEndTitle, lblEndX, txtEndX, lblEndY, txtEndY,
                btnOk, btnCancel
            });

            // Устанавливаем фокус на первое поле ввода
            this.ActiveControl = txtStartX;
        }

        private void BtnOk_Click(object sender, EventArgs e)
        {
            if (float.TryParse(txtStartX.Text, out float sx) &&
                float.TryParse(txtStartY.Text, out float sy) &&
                float.TryParse(txtEndX.Text, out float ex) &&
                float.TryParse(txtEndY.Text, out float ey))
            {
                Edge = new Edge(new PointF(sx, sy), new PointF(ex, ey));
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                MessageBox.Show("Введите корректные числовые координаты", "Ошибка ввода",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}