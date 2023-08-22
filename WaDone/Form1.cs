using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace WaDone
{
    public partial class Cb_Trans_Count : Form
    {
        public Cb_Trans_Count()
        {
            InitializeComponent();
            GetTotalCount();
        }

        private List<(List<EProperity>, List<int>)> ResultAnswer = new List<(List<EProperity>, List<int>)>();
        private List<(List<int>, List<int>)> Result = new List<(List<int>, List<int>)>();

        // 起始
        private int StartProperity = 0;

        private int StartEnergy = 0;

        // 中間篩選
        private EProperity MiddleProperity;

        private int MiddlePath = 0;

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
        private int Length = 5;

        private int[,] Maze = null;
        private Dictionary<(int, int), int> PathRecord = new Dictionary<(int, int), int>();
        private ERotate EndRotate = ERotate.橫;

        private void Btn_Start_Click(object sender, EventArgs e)
        {
            Init();
            GetStartEnd();
            ResultTable = new DataTable();

            Go(StartProperity, StartEnergy, 0, 0, 0, 0, 0, 0, new List<EProperity> { (EProperity)StartProperity });
            GetTotalCount();
        }

        private void Go(int startPro, int startEng, int wood, int fire, int dust, int gold, int water, int path, List<EProperity> process)
        {
            path++;
            EProperity start = (EProperity)startPro;
            if (path < PathCount + 1 && ResultTable.Rows.Count == 0)
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
                            // 篩選路徑 等於0 不檢查
                            if (MiddlePath == 0)
                            {
                                tempProcess.Add(encome);
                                Go(tempStartPro, tempStartEng, tempWood, tempFire, tempDust, tempGold, tempWood, path, tempProcess);
                            }
                            else
                            {
                                if (MiddlePath == path)
                                {
                                    if (encome == MiddleProperity)
                                    {
                                        tempProcess.Add(encome);
                                        Go(tempStartPro, tempStartEng, tempWood, tempFire, tempDust, tempGold, tempWood, path, tempProcess);
                                    }
                                }
                                else
                                {
                                    tempProcess.Add(encome);
                                    Go(tempStartPro, tempStartEng, tempWood, tempFire, tempDust, tempGold, tempWood, path, tempProcess);
                                }
                            }
                        }
                    }
                }
            }
            else if (path == PathCount + 1)
            {
                if (startPro == EndProperity && startEng == EndEnergy)
                {
                    bool isAdd = false;

                    List<EProperity> tempProcess = process.Skip(1).ToList();

                    if (Total_Properity_Lenth != 0 || TransCount != 0)
                    {
                        // 至少某屬性幾次
                        if (Total_Properity_Lenth != 0)
                        {
                            if (tempProcess.Where(x => x == Total_Properity).Count() >= Total_Properity_Lenth)
                            {
                                isAdd = true;
                            }
                        }
                        if (TransCount != 0)
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
                                isAdd = true;
                            }
                        }
                    }
                    else
                    {
                        isAdd = true;
                    }
                    if (isAdd)
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
            // 五屬性數量
            WoodCount = int.Parse(Tb_Wood_Count.Text);
            FireCount = int.Parse(Tb_Fire_Count.Text);
            DustCount = int.Parse(Tb_Dust_Count.Text);
            GoldCount = int.Parse(Tb_Gold_Count.Text);
            WaterCount = int.Parse(Tb_Water_Count.Text);

            // 五屬性剩餘數量
            WoodTransCount = int.Parse(Tb_Wood_Trans_Count.Text);
            FireTransCount = int.Parse(Tb_Fire_Trans_Count.Text);
            DustTransCount = int.Parse(Tb_Dust_Trans_Count.Text);
            GoldTransCount = int.Parse(Tb_Gold_Trans_Count.Text);
            WaterTransCount = int.Parse(Tb_Water_Trans_Count.Text);

            WoodStrageCount = WoodCount - WoodTransCount;
            FireStrageCount = FireCount - FireTransCount;
            DustStrageCount = DustCount - DustTransCount;
            GoldStrageCount = GoldCount - GoldTransCount;
            WaterStrageCount = WaterCount - WaterTransCount;

            //路徑數量
            PathCount = int.Parse(Tb_Path_Count.Text);
            MiddlePath = int.Parse(Tb_Middle_Path.Text);
            TransCount = int.Parse(Tb_Trans_Count.Text);

            Total_Properity_Lenth = int.Parse(Tb_Len_Count.Text);

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
            MiddleProperity = (EProperity)NameToInt[Cb_Middle_Properity.Text];
            EndProperity = NameToInt[Cb_End_Properity.Text];
            Total_Properity = (EProperity)NameToInt[Cb_Len_Properity.Text];

            // 能量
            StartEnergy = int.Parse(Tb_Start_Energy.Text);
            EndEnergy = int.Parse(Tb_End_Energy.Text);

            // 終迄2點
            Start_X = int.Parse(Tb_Start_x.Text);
            Start_Y = int.Parse(Tb_Start_y.Text);
            Start1_X = int.Parse(Tb_Start1_x.Text);
            Start1_Y = int.Parse(Tb_Start1_y.Text);

            End_X = int.Parse(Tb_End_x.Text);
            End_Y = int.Parse(Tb_End_y.Text);
            End1_X = int.Parse(Tb_End1_x.Text);
            End1_Y = int.Parse(Tb_End1_y.Text);

            PathCount = int.Parse(Tb_Path_Count.Text);
        }

        private void GetTotalCount() => ToTalCount.Text = $"總共:{ResultTable.Rows.Count}個";

        private void GetStartEnd()
        {
            ERotate startRotate = (Start1_X - Start_X == 0) ? ERotate.直 : ERotate.橫;
            ERotate endRotate = (End1_X - End_X == 0) ? ERotate.直 : ERotate.橫;

            Result = new List<(List<int>, List<int>)>();
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
            var totalNeed = source.Skip(1).GroupBy(x => x).Select(prop => new { properity = prop.Key, count = prop.Count() }).ToList();

            foreach ((List<int> path, List<int> trans) item in Result)
            {
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
                                if (goldTransCount > DustTransCount)
                                {
                                    tempAnswer = false;
                                }
                            }

                            // 直線判斷
                            if (DustStrageCount < (perNeed.count - goldTransCount))
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
                            if (WaterStrageCount < (WaterCount - waterTransCount))
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
                   List<int> transValues =  item.trans.Select(x => x + 1).ToList();
                    source.RemoveAt(0);
                    ResultAnswer.Add((source.Skip(1).ToList(), item.path));
                    ResultTable.Columns.Add("屬性");
                    ResultTable.Columns.Add("路徑");
                    for (int i = 0; i < source.Count; i++)
                    {
                        DataRow row = ResultTable.NewRow();
                        row[0] = source[i].ToString();
                        string  v = (transValues.Contains( item.path[i]))?
                                        ",轉彎":
                                        string.Empty;
                        row[1] = $@"{item.path[i]}{v}";
                        ResultTable.Rows.Add(row);
                    }
                    new Action(() => dataGridView1.DataSource = ResultTable)();
                    break;
                }
            }
        }
    }
}