using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Management;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace WaDone
{
    public partial class Cb_Trans_Count : Form
    {
        public Cb_Trans_Count()
        {
            InitializeComponent();
            CheckDate();
        }

        private List<(List<EProperity>, List<int>)> ResultAnswer = new List<(List<EProperity>, List<int>)>();
        private List<(List<int>, List<int>)> Result = new List<(List<int>, List<int>)>();

        // 起始
        private int StartProperity = 0;

        private int StartEnergy = 0;

        // 結束
        private int EndProperity = 0;

        private int EndEnergy = 0;

        // 總路徑
        private int PathCount = 0;

        // 剩餘五屬數量
        private int WoodCount = 0;

        private int FireCount = 0;
        private int DustCount = 0;
        private int GoldCount = 0;
        private int WaterCount = 0;

        // 剩餘五屬直線數量
        private int WoodStrageCount = 0;

        private int FireStrageCount = 0;
        private int DustStrageCount = 0;
        private int GoldStrageCount = 0;
        private int WaterStrageCount = 0;

        // 剩餘五屬轉彎數量
        private int WoodTransCount = 0;

        private int FireTransCount = 0;
        private int DustTransCount = 0;
        private int GoldTransCount = 0;
        private int WaterTransCount = 0;

        // 至少某屬性幾個
        private EProperity Total_Properity;

        private int Total_Properity_Lenth = 0;

        // 轉屬幾次
        private int TransCount = 0;

        private Dictionary<string, int> NameToInt = new Dictionary<string, int>();
        private Dictionary<EProperity, int> ProperityToInt = new Dictionary<EProperity, int>();

        public DataTable ResultTable = new DataTable();

        // 算路徑
        private int Start_X = 0;

        private int Start_Y = 0;

        private int Start1_X = 0;
        private int Start1_Y = 0;

        private int End_X = 0;
        private int End_Y = 0;

        private int End1_X = 0;
        private int End1_Y = 0;

        // 設定
        private static int Length = 5;

        private int[,] Maze = null;
        private Dictionary<(int, int), int> PathRecord = new Dictionary<(int, int), int>();
        private ERotate EndRotate = ERotate.橫;

        private int CurrentPoint = 0;
        private int CurrentProperity = 0;

        private string ErrorMessage = string.Empty;
        private bool HasAnswer = false;
        private List<EProperity> ResultProperityPath = new List<EProperity>();
        private List<int> ResultPath = new List<int>();
        private List<int> ResultTransPath = new List<int>();

        private Dictionary<int, (int x, int y)> ParseCoor = Enumerable.Range(0, Length).SelectMany(x => Enumerable.Range(0, Length), (x, y) => (x, y))
                                                                 .ToDictionary(
                                                                       index => (index.x + 1) + (Length * index.y),
                                                                       coordinate => (coordinate.x, coordinate.y));

        private int PathLimit = 0;

        private void Btn_Start_Click(object sender, EventArgs e)
        {
            Init();
            ResultTable = new DataTable();
            Enumerable.Range(1, Length).ToList().ForEach(x => ResultTable.Columns.Add(x.ToString()));
            Enumerable.Range(1, Length).ToList().ForEach(x => ResultTable.Rows.Add(ResultTable.NewRow()));
            dataGridView1.DataSource = ResultTable;
            dataGridView1.ClearSelection();

            if (string.IsNullOrEmpty(ErrorMessage))
            {
                bool isLoop = true;
                while (isLoop && PathCount <= Math.Pow(Length, 2) - 2)
                {
                    GetStartEnd();
                    if (Result.Any())
                    {
                        int diffPathCount = PathCount + 1;
                        Go(StartProperity, StartEnergy, 0, 0, 0, 0, 0, 0, new List<EProperity> { (EProperity)StartProperity }, diffPathCount);

                        if (HasAnswer)
                        {
                            dataGridView1.DataSource = ResultTable;
                            TransAnswer();
                            dataGridView1.ClearSelection();
                        }
                    }
                    if (HasAnswer || !CB_NeedResult.Checked)
                    {
                        isLoop = false;
                    }
                    PathCount = PathCount + 2;
                    Tb_Path_Count.Text = PathCount.ToString();
                }
                if (!HasAnswer)
                {
                    ErrorMessage = "無解答";
                }
            }
            label1.Text = ErrorMessage;
        }

        /// <summary>
        /// 結果顯示轉換
        /// </summary>
        private void TransAnswer()
        {
            Dictionary<EProperity, Color> changeColor = new Dictionary<EProperity, Color>
            {
                { EProperity.木,Color.Lime},
                { EProperity.火,Color.Red},
                { EProperity.金,Color.Yellow},
                { EProperity.土,Color.Chocolate},
                { EProperity.水,Color.Cyan}
            };

            // 設定起訖顏色
            dataGridView1.Rows[Start_Y].Cells[Start_X].Style.BackColor = changeColor[(EProperity)StartProperity];
            dataGridView1.Rows[End_Y].Cells[End_X].Style.BackColor = changeColor[(EProperity)EndProperity];
            for (int i = 0; i < ResultPath.Count; i++)
            {
                (int x, int y) = ParseCoor[ResultPath[i]];
                dataGridView1.Rows[y].Cells[x].Style.BackColor = changeColor[ResultProperityPath[i]];
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="startPro"></param>
        /// <param name="startEng"></param>
        /// <param name="wood"></param>
        /// <param name="fire"></param>
        /// <param name="dust"></param>
        /// <param name="gold"></param>
        /// <param name="water"></param>
        /// <param name="path"></param>
        /// <param name="process"></param>
        /// <param name="diffPathCount">還有幾步路到終點</param>
        private void Go(int startPro, int startEng, int wood, int fire, int dust, int gold, int water, int path, List<EProperity> process, int diffPathCount)
        {
            diffPathCount--;
            path++;
            EProperity start = (EProperity)startPro;

            int diffProperityPower = EndProperity - startPro;
            if (diffProperityPower < 0)
            { diffProperityPower += Length; }

            int diffEnergy = Math.Abs(startEng - EndEnergy);
            int totalDiffPower = diffProperityPower + diffEnergy;

            if (path < PathCount + 1 && !HasAnswer && (totalDiffPower <= diffPathCount))
            {
                for (int i = 0; i < 5; i++)
                {
                    int tempStartPro = startPro;
                    int tempStartEng = startEng;
                    int tempWood = wood;
                    int tempFire = fire;
                    int tempDust = dust;
                    int tempGold = gold;
                    int tempWater = water;

                    bool isRun = false;
                    List<EProperity> tempProcess = process.ToList();

                    EProperity encome = (EProperity)i;
                    switch (encome)
                    {
                        case EProperity.木:
                            tempWood++;
                            if (tempWood <= WoodCount)
                            {
                                isRun = true;
                                // 一開始屬性 遇到木
                                switch (start)
                                {
                                    case EProperity.木:
                                        break;

                                    case EProperity.火:
                                        tempStartEng++;
                                        break;

                                    case EProperity.土:
                                        tempStartEng--;
                                        break;

                                    case EProperity.金:
                                        tempStartEng--;
                                        break;

                                    case EProperity.水:
                                        tempStartPro = (int)EProperity.木;
                                        break;

                                    default:
                                        break;
                                }
                            }
                            break;

                        case EProperity.火:
                            tempFire++;
                            if (tempFire <= FireCount)
                            {
                                isRun = true;
                                // 一開始屬性 遇到火
                                switch (start)
                                {
                                    case EProperity.木:
                                        tempStartPro = (int)EProperity.火;
                                        break;

                                    case EProperity.火:
                                        break;

                                    case EProperity.土:
                                        tempStartEng++;
                                        break;

                                    case EProperity.金:
                                        tempStartEng--;
                                        break;

                                    case EProperity.水:
                                        tempStartEng--;
                                        break;
                                }
                            }
                            break;

                        case EProperity.土:
                            tempDust++;
                            if (tempDust <= DustCount)
                            {
                                isRun = true;
                                // 一開始屬性 遇到土
                                switch (start)
                                {
                                    case EProperity.木:
                                        tempStartEng--;
                                        break;

                                    case EProperity.火:
                                        tempStartPro = (int)EProperity.土;
                                        break;

                                    case EProperity.土:
                                        break;

                                    case EProperity.金:
                                        tempStartEng++;
                                        break;

                                    case EProperity.水:
                                        tempStartEng--;
                                        break;
                                }
                            }
                            break;

                        case EProperity.金:
                            tempGold++;
                            if (tempGold <= GoldCount)
                            {
                                isRun = true;
                                // 一開始屬性 遇到金
                                switch (start)
                                {
                                    case EProperity.木:
                                        tempStartEng--;
                                        break;

                                    case EProperity.火:
                                        tempStartEng--;
                                        break;

                                    case EProperity.土:
                                        tempStartPro = (int)EProperity.金;
                                        break;

                                    case EProperity.金:
                                        break;

                                    case EProperity.水:
                                        tempStartEng++;
                                        break;
                                }
                            }
                            break;

                        case EProperity.水:
                            tempWater++;
                            if (tempWater <= WaterCount)
                            {
                                isRun = true;
                                // 一開始屬性 遇到水
                                switch (start)
                                {
                                    case EProperity.木:
                                        tempStartEng++;
                                        break;

                                    case EProperity.火:
                                        tempStartEng--;
                                        break;

                                    case EProperity.土:
                                        tempStartEng--;
                                        break;

                                    case EProperity.金:
                                        tempStartPro = (int)EProperity.水;
                                        break;

                                    case EProperity.水:
                                        break;
                                }
                            }
                            break;
                    }

                    if (isRun)
                    {
                        if (tempStartEng != 0 && tempStartEng != 4)
                        {
                            tempProcess.Add(encome);
                            Go(tempStartPro, tempStartEng, tempWood, tempFire, tempDust, tempGold, tempWood, path, tempProcess, diffPathCount);
                        }
                    }
                }
            }
            else if (path == PathCount + 1)
            {
                if (startPro == EndProperity && startEng == EndEnergy)
                {
                    // 檢查 [某屬性幾次, 轉數幾次]
                    bool[] isAllMatch = new bool[] { false, false };
                    // 至少某屬性幾次
                    if (Total_Properity_Lenth == 0)
                    {
                        isAllMatch[0] = true;
                    }
                    else
                    {
                        List<EProperity> tempProcess = process.ToList();
                        tempProcess.Add((EProperity)EndProperity);
                        if (tempProcess.Where(x => x == Total_Properity).Count() >= Total_Properity_Lenth)
                        {
                            isAllMatch[0] = true;
                        }
                    }

                    // 轉屬幾次
                    if (TransCount == 0)
                    {
                        isAllMatch[1] = true;
                    }
                    else
                    {
                        int maxCount = 0;
                        int lastValue = ProperityToInt.Last().Value;
                        List<EProperity> tempTransList = process.ToList();
                        tempTransList.Add((EProperity)EndProperity);

                        int properityRecord = ProperityToInt[tempTransList.First()];
                        var _ = tempTransList.Aggregate((x, y) =>
                        {
                            int source = ProperityToInt[x];
                            int target = (ProperityToInt[y] == ProperityToInt.First().Value) ?
                                                    lastValue + 1 :
                                                    ProperityToInt[y];
                            if (source == properityRecord && source + 1 == target)
                            {
                                // 轉屬
                                maxCount++;
                                properityRecord = (target == lastValue + 1) ? 0 : target;
                            }
                            return y;
                        });
                        if (maxCount >= TransCount)
                        {
                            isAllMatch[1] = true;
                        }
                    }
                    if (isAllMatch.All(x => x))
                    {
                        CheckAnswer(process);
                    }
                }
            }
        }

        /// <summary>
        /// 初始化
        /// </summary>
        private void Init()
        {
            HasAnswer = false;
            ErrorMessage = string.Empty;

            // 五屬性數量
            WoodCount = GetDefaultValue(Tb_Wood_Count.Text);
            FireCount = GetDefaultValue(Tb_Fire_Count.Text);
            DustCount = GetDefaultValue(Tb_Dust_Count.Text);
            GoldCount = GetDefaultValue(Tb_Gold_Count.Text);
            WaterCount = GetDefaultValue(Tb_Water_Count.Text);

            // 五屬性剩餘數量
            WoodTransCount = GetDefaultValue(Tb_Wood_Trans_Count.Text);
            FireTransCount = GetDefaultValue(Tb_Fire_Trans_Count.Text);
            DustTransCount = GetDefaultValue(Tb_Dust_Trans_Count.Text);
            GoldTransCount = GetDefaultValue(Tb_Gold_Trans_Count.Text);
            WaterTransCount = GetDefaultValue(Tb_Water_Trans_Count.Text);

            WoodStrageCount = WoodCount - WoodTransCount;
            FireStrageCount = FireCount - FireTransCount;
            DustStrageCount = DustCount - DustTransCount;
            GoldStrageCount = GoldCount - GoldTransCount;
            WaterStrageCount = WaterCount - WaterTransCount;

            //路徑數量
            PathCount = GetDefaultValue(Tb_Path_Count.Text) - 2;
            TransCount = GetDefaultValue(Tb_Trans_Count.Text);

            if (PathCount % 2 != PathLimit % 2)
            {
                ErrorMessage = "請保持一樣的奇偶數";
            }

            Total_Properity_Lenth = GetDefaultValue(Tb_Len_Count.Text);

            // 轉換
            NameToInt = Enum.GetNames(typeof(EProperity))
                            .Select((x, i) => new { value = x, Index = i })
                            .ToDictionary(
                                key => key.value,
                                value => value.Index);

            ProperityToInt = ((EProperity[])Enum.GetValues(typeof(EProperity)))
                                                .ToDictionary(
                                                    key => key,
                                                    index => (int)index);

            // 屬性
            StartProperity = NameToInt[Cb_Start_Properity.Text];
            EndProperity = NameToInt[Cb_End_Properity.Text];
            Total_Properity = (EProperity)NameToInt[Cb_Len_Properity.Text];

            // 能量
            StartEnergy = GetDefaultValue(Tb_Start_Energy.Text);
            EndEnergy = GetDefaultValue(Tb_End_Energy.Text);

            // 終迄2點
            Start_X = GetDefaultValue(Tb_Start_x.Text) - 1;
            Start_Y = GetDefaultValue(Tb_Start_y.Text) - 1;
            Start1_X = GetDefaultValue(Tb_Start1_x.Text) - 1;
            Start1_Y = GetDefaultValue(Tb_Start1_y.Text) - 1;

            End_X = GetDefaultValue(Tb_End_x.Text) - 1;
            End_Y = GetDefaultValue(Tb_End_y.Text) - 1;
            End1_X = GetDefaultValue(Tb_End1_x.Text) - 1;
            End1_Y = GetDefaultValue(Tb_End1_y.Text) - 1;
        }

        private void GetStartEnd()
        {
            ERotate startRotate = (Start1_X - Start_X == 0) ? ERotate.直 : ERotate.橫;
            EndRotate = (End1_X - End_X == 0) ? ERotate.直 : ERotate.橫;

            Result = new List<(List<int>, List<int>)>();
            ResultAnswer = new List<(List<EProperity>, List<int>)>();
            Maze = new int[Length, Length];
            List<int> maxLength = Enumerable.Range(0, Length).ToList();
            PathRecord = maxLength.SelectMany(x => maxLength, (x, y) => (x, y)).ToDictionary(
                           key => (key.x, key.y),
                           value => value.x + (value.y * Length) + 1);

            // 設定 起始終迄點
            Maze[Start_X, Start_Y] = 1;
            Maze[End_X, End_Y] = 1;

            // 設定起始走點
            SolveMaze(Start1_X, Start1_Y, startRotate, new List<int>(), new List<int>());

            // 判斷轉彎數
            int totalTransCount = WoodTransCount + FireTransCount + DustTransCount + GoldTransCount + WaterTransCount;
            Result = Result.Where(x => x.Item2.Count <= totalTransCount).ToList();
            if (PathCount == Math.Pow(Length, 2))
            {
                if (startRotate == EndRotate)
                {
                    if (totalTransCount % 2 != 0)
                    {
                        Result = new List<(List<int>, List<int>)>();
                        ErrorMessage = "轉彎數不對";
                    }
                }
                else
                {
                    if (totalTransCount % 2 == 0)
                    {
                        Result = new List<(List<int>, List<int>)>();
                        ErrorMessage = "轉彎數不對";
                    }
                }
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="row"> X </param>
        /// <param name="col"> Y </param>
        /// <param name="path"></param>
        private void SolveMaze(int row, int col, ERotate rotate, List<int> path, List<int> rotateRecord)
        {
            //設定結束位置
            if (row == End1_X && col == End1_Y)
            {
                // Reached the end of the maze
                path.Add(PathRecord[(row, col)]);

                if (path.Count == PathCount)
                {
                    if (rotate != EndRotate)
                    {
                        rotateRecord.Add(PathRecord[(row, col)]);
                    }
                    Result.Add((path, rotateRecord.Select(x => x - 1).ToList()));
                }
                return;
            }

            if (row >= 0 && row < Length && col >= 0 && col < Length && Maze[row, col] == 0)
            {
                Maze[row, col] = 1;  // Mark the current cell as visited
                path.Add(PathRecord[(row, col)]);

                ERotate tempRotate = ERotate.直;

                // Try moving in all possible directions (right, down, left, up)
                // col Y
                List<int> tempPath = path.ToList();
                List<int> tempRotateList = rotateRecord.ToList();
                if (rotate != tempRotate)
                {
                    tempRotateList.Add(PathRecord[(row, col)]);
                }
                SolveMaze(row, col + 1, tempRotate, tempPath, tempRotateList); // Right

                tempPath = path.ToList();
                SolveMaze(row, col - 1, tempRotate, tempPath, tempRotateList); // Left

                // row X
                tempRotate = ERotate.橫;
                tempPath = path.ToList();
                tempRotateList = rotateRecord.ToList();
                if (rotate != tempRotate)
                {
                    tempRotateList.Add(PathRecord[(row, col)]);
                }
                SolveMaze(row + 1, col, tempRotate, tempPath, tempRotateList); // Down

                tempPath = path.ToList();
                SolveMaze(row - 1, col, tempRotate, tempPath, tempRotateList); // Up

                Maze[row, col] = 0;  // Reset the current cell for backtracking
            }
        }

        private void CheckAnswer(List<EProperity> source)
        {
            // 總路徑 所需個屬性需要的數量
            var totalNeed = source.Skip(1).GroupBy(x => x).Select(prop => new { properity = prop.Key, count = prop.Count() }).ToList();

            foreach ((List<int> path, List<int> trans) item in Result)
            {
                // 總轉彎 所需屬性的數量
                Dictionary<int, int> transToIndex = item.path.Select((data, i) => new { value = data, index = i }).ToDictionary(x => x.value - 1, y => y.index + 1);

                var transDetail = item.trans.Select(data => source[transToIndex[data]]).GroupBy(data => data).ToDictionary(
                    properity => properity.Key,
                    count => count.Count()
                );

                bool isAnswer = true;
                foreach (var perNeed in totalNeed)
                {
                    bool tempAnswer = true;
                    switch (perNeed.properity)
                    {
                        case EProperity.木:
                            // 轉彎判斷
                            if (transDetail.TryGetValue(perNeed.properity, out int woodTransCount))
                            {
                                if (woodTransCount > WoodTransCount)
                                {
                                    tempAnswer = false;
                                }
                            }

                            // 直線判斷
                            if (WoodStrageCount < (perNeed.count - woodTransCount))
                            {
                                tempAnswer = false;
                            }
                            break;

                        case EProperity.火:
                            // 轉彎判斷
                            if (transDetail.TryGetValue(perNeed.properity, out int fireTransCount))
                            {
                                if (fireTransCount > FireTransCount)
                                {
                                    tempAnswer = false;
                                }
                            }

                            // 直線判斷
                            if (FireStrageCount < (perNeed.count - fireTransCount))
                            {
                                tempAnswer = false;
                            }
                            break;

                        case EProperity.土:
                            // 轉彎判斷
                            if (transDetail.TryGetValue(perNeed.properity, out int dustTransCount))
                            {
                                if (dustTransCount > DustTransCount)
                                {
                                    tempAnswer = false;
                                }
                            }

                            // 直線判斷
                            if (DustStrageCount < (perNeed.count - dustTransCount))
                            {
                                tempAnswer = false;
                            }
                            break;

                        case EProperity.金:
                            // 轉彎判斷
                            if (transDetail.TryGetValue(perNeed.properity, out int goldTransCount))
                            {
                                if (goldTransCount > GoldTransCount)
                                {
                                    tempAnswer = false;
                                }
                            }

                            // 直線判斷
                            if (GoldStrageCount < (perNeed.count - goldTransCount))
                            {
                                tempAnswer = false;
                            }
                            break;

                        case EProperity.水:
                            // 轉彎判斷
                            if (transDetail.TryGetValue(perNeed.properity, out int waterTransCount))
                            {
                                if (waterTransCount > WaterTransCount)
                                {
                                    tempAnswer = false;
                                }
                            }

                            // 直線判斷
                            if (WaterStrageCount < (perNeed.count - waterTransCount))
                            {
                                tempAnswer = false;
                            }
                            break;
                    }
                    if (!tempAnswer)
                    {
                        isAnswer = false;
                    }
                }
                if (isAnswer)
                {
                    ResultTransPath = item.trans.Select(x => x + 1).ToList();
                    source.RemoveAt(0);
                    ResultAnswer.Add((source.Skip(1).ToList(), item.path));
                    HasAnswer = true;
                    ResultProperityPath = source;
                    ResultPath = item.path;
                    // 設定轉彎
                    Dictionary<(int, int), string> trans = new Dictionary<(int, int), string>
                    {
                        { (5,1),"└"},
                        { (5,-1),"┘"},
                        { (1,-5),"┘"},
                        { (1,5),"┐"},
                        { (-5,1),"┌"},
                        { (-5,-1),"┐"},
                        { (-1,-5),"└"},
                        { (-1,5),"┌"},
                    };
                    // 加入起訖
                    List<int> tempPath = new List<int> { PathRecord[(Start_X, Start_Y)] };
                    tempPath.AddRange(ResultPath);
                    tempPath.Add(PathRecord[(End_X, End_Y)]);

                    // 加入起訖文字
                    ResultTable.Rows[Start_Y][Start_X] = "🐭";
                    ResultTable.Rows[End_Y][End_X] = "🧀";

                    foreach (int perTrans in ResultTransPath)
                    {
                        int transIndex = tempPath.IndexOf(perTrans);
                        int diffSource = tempPath[transIndex] - tempPath[transIndex - 1];
                        int diffTarget = tempPath[transIndex + 1] - tempPath[transIndex];

                        (int x, int y) = ParseCoor[perTrans];
                        ResultTable.Rows[y][x] = trans[(diffSource, diffTarget)];
                    }
                    break;
                }
            }
        }

        private void SetPoint(object sender)
        {
            Button btn = (Button)sender;
            string[] xy = btn.Text.Split(',');
            switch (CurrentPoint)
            {
                case 0:
                    Tb_Start_x.Text = xy[0];
                    Tb_Start_y.Text = xy[1];
                    CurrentPoint++;
                    break;

                case 1:
                    Tb_Start1_x.Text = xy[0];
                    Tb_Start1_y.Text = xy[1];
                    CurrentPoint++;
                    break;

                case 2:
                    Tb_End_x.Text = xy[0];
                    Tb_End_y.Text = xy[1];
                    CurrentPoint++;
                    break;

                case 3:
                    Tb_End1_x.Text = xy[0];
                    Tb_End1_y.Text = xy[1];

                    int diffX = Math.Abs(int.Parse(Tb_End1_x.Text) - int.Parse(Tb_Start1_x.Text));
                    int diffY = Math.Abs(int.Parse(Tb_End1_y.Text) - int.Parse(Tb_Start1_y.Text));
                    PathLimit = diffY + diffX + 3;
                    Tb_Path_Count.Text = PathLimit.ToString();
                    CurrentPoint = 0;
                    break;

                default:
                    break;
            }
        }

        /// <summary>
        /// 1
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button4_Click(object sender, EventArgs e) => SetPoint(sender);

        private void button3_Click(object sender, EventArgs e) => SetPoint(sender);

        private void button8_Click(object sender, EventArgs e) => SetPoint(sender);

        private void button7_Click(object sender, EventArgs e) => SetPoint(sender);

        private void button6_Click(object sender, EventArgs e) => SetPoint(sender);

        /// <summary>
        /// 2
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button11_Click(object sender, EventArgs e) => SetPoint(sender);

        private void button12_Click(object sender, EventArgs e) => SetPoint(sender);

        private void button5_Click(object sender, EventArgs e) => SetPoint(sender);

        private void button9_Click(object sender, EventArgs e) => SetPoint(sender);

        private void button10_Click(object sender, EventArgs e) => SetPoint(sender);

        /// <summary>
        /// 3
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button16_Click(object sender, EventArgs e) => SetPoint(sender);

        private void button17_Click(object sender, EventArgs e) => SetPoint(sender);

        private void button13_Click(object sender, EventArgs e) => SetPoint(sender);

        private void button14_Click(object sender, EventArgs e) => SetPoint(sender);

        private void button15_Click(object sender, EventArgs e) => SetPoint(sender);

        /// <summary>
        /// 4
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button21_Click(object sender, EventArgs e) => SetPoint(sender);

        private void button22_Click(object sender, EventArgs e) => SetPoint(sender);

        private void button18_Click(object sender, EventArgs e) => SetPoint(sender);

        private void button19_Click(object sender, EventArgs e) => SetPoint(sender);

        private void button20_Click(object sender, EventArgs e) => SetPoint(sender);

        /// <summary>
        /// 5
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button26_Click(object sender, EventArgs e) => SetPoint(sender);

        private void button27_Click(object sender, EventArgs e) => SetPoint(sender);

        private void button23_Click(object sender, EventArgs e) => SetPoint(sender);

        private void button24_Click(object sender, EventArgs e) => SetPoint(sender);

        private void button25_Click(object sender, EventArgs e) => SetPoint(sender);

        /// <summary>
        /// 起訖屬性
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e) => GetStartProperity(sender);

        private void button30_Click(object sender, EventArgs e) => GetStartProperity(sender);

        private void button29_Click(object sender, EventArgs e) => GetStartProperity(sender);

        private void button28_Click(object sender, EventArgs e) => GetStartProperity(sender);

        private void button2_Click(object sender, EventArgs e) => GetStartProperity(sender);

        private void GetStartProperity(object sender)
        {
            Button btn = (Button)sender;
            string text = btn.Text;
            switch (CurrentProperity)
            {
                case 0:
                    Cb_Start_Properity.Text = text;
                    CurrentProperity++;
                    break;

                case 1:
                    Cb_End_Properity.Text = text;
                    CurrentProperity = 0;
                    Tb_Start_Energy.Focus();
                    break;
            }
        }

        private void Tb_Gold_Trans_Count_TextChanged(object sender, EventArgs e)
        {
            try
            {
                List<int> source = new List<int>
                {
                    int.Parse(Tb_Wood_Count.Text),
                    int.Parse(Tb_Fire_Count.Text),
                    int.Parse(Tb_Gold_Count.Text),
                    int.Parse(Tb_Dust_Count.Text),
                };
                Tb_Water_Count.Text = (Math.Pow(Length, 2) - 2 - source.Sum()).ToString();
                Tb_Water_Trans_Count.Focus();
            }
            catch
            {
                Tb_Water_Count.Text = string.Empty;
            }
        }

        private int GetDefaultValue(string source) => int.TryParse(source, out int value) ? value : value;

        private void button31_Click(object sender, EventArgs e)
        {
            Tb_Start_Energy.Text = string.Empty;
            Tb_End_Energy.Text = string.Empty;

            Tb_Wood_Count.Text = string.Empty;
            Tb_Wood_Trans_Count.Text = string.Empty;

            Tb_Fire_Count.Text = string.Empty;
            Tb_Fire_Trans_Count.Text = string.Empty;

            Tb_Water_Count.Text = string.Empty;
            Tb_Water_Trans_Count.Text = string.Empty;

            Tb_Gold_Count.Text = string.Empty;
            Tb_Gold_Trans_Count.Text = string.Empty;

            Tb_Dust_Count.Text = string.Empty;
            Tb_Dust_Trans_Count.Text = string.Empty;

            Tb_Len_Count.Text = string.Empty;
            Tb_Trans_Count.Text = string.Empty;
        }

        private void Tb_Start_Energy_TextChanged(object sender, EventArgs e) => Tb_End_Energy.Focus();

        private void Tb_End_Energy_TextChanged(object sender, EventArgs e) => Tb_Wood_Count.Focus();

        private void Tb_Wood_Count_TextChanged(object sender, EventArgs e) => Tb_Wood_Trans_Count.Focus();

        private void Tb_Wood_Trans_Count_TextChanged(object sender, EventArgs e) => Tb_Fire_Count.Focus();

        private void Tb_Fire_Count_TextChanged(object sender, EventArgs e) => Tb_Fire_Trans_Count.Focus();

        private void Tb_Fire_Trans_Count_TextChanged(object sender, EventArgs e) => Tb_Dust_Count.Focus();

        private void Tb_Dust_Count_TextChanged(object sender, EventArgs e) => Tb_Dust_Trans_Count.Focus();

        private void Tb_Dust_Trans_Count_TextChanged(object sender, EventArgs e) => Tb_Gold_Count.Focus();

        private void Tb_Gold_Count_TextChanged(object sender, EventArgs e) => Tb_Gold_Trans_Count.Focus();

        private void CheckDate()
        {
            HttpClient client = new HttpClient();
            string checkDate = client.GetStringAsync($"https://xujpvuxu.github.io/WaDone/CheckDate.json").Result;
            CheckDate source = JsonConvert.DeserializeObject<CheckDate>(checkDate);
            label1.Text = $"最後更新日期為：{source.Date}";
        }

        private void label1_Click(object sender, EventArgs e)
        {
            Process.Start($"https://xujpvuxu.github.io/WaDone/WaDone.zip");
        }
    }
}