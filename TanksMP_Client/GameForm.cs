using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static TanksMP_Client.Form1;

namespace TanksMP_Client
{
    public partial class GameForm : Form
    {
        private const string apiUrl = "https://localhost:44319/";
        Player mePlayer = new Player();
        static HttpClient client = new HttpClient();
        public List<Player> players = new List<Player>();
        public Map map;
        List<IBlock> blocks = new List<IBlock>();
        Random rnd = new Random();
        Color pColor;
        int mapSizeX = 20;
        Pen p = new Pen(Color.Black);
        SolidBrush sb = new SolidBrush(Color.Red);
        Graphics g = null;
        PictureBox[] playerPics = new PictureBox[50];
        PictureBox playerPic;

        public GameForm()
        {
            InitializeComponent();
            this.DoubleBuffered = true;
            this.ControlBox = false;
            g = panel2.CreateGraphics();
            p = new Pen(Color.Black);
            sb = new SolidBrush(Color.Red);
        }

        //Event handlers
        private async void joinBtn_Click(object sender, EventArgs e)
        {
            await joinGameAsync();
            Timer timer = new Timer();
            timer.Interval = (10); // 0.5 secs
            timer.Tick += new EventHandler(timer_Tick);
            timer.Start();
        }

        private async void timer_Tick(object sender, EventArgs e)
        {
            ICollection<Player> playersTemp = await GetAllPlayerAsync(client.BaseAddress.PathAndQuery);
            players = playersTemp.ToList();
            UpdatePlayers();
        }

        private void UpdatePlayers()
        {
            foreach (Player p in players)
            {
                if (!isPlayerPicCreated(p.Id))
                {
                    PictureBox pb = createPlayerPic(p);
                    panel2.Controls.Add(pb);
                    playerPics[p.Id] = pb;
                }
                playerPics[p.Id].Location = new Point(p.getPosX() * mapSizeX, p.getPosY() * mapSizeX);
                playerPics[p.Id].Image = Image.FromFile("..\\..\\Images\\tank" + p.Rotation + ".png");
            }
        }

        private PictureBox createPlayerPic(Player p)
        {
            string path = "..\\..\\Images\\tank" + p.Rotation + ".png";
            PictureBox pic = new PictureBox
            {
                Location = new Point(10, 10),
                Image = Image.FromFile(path),
                Width = mapSizeX,
                Height = mapSizeX,
                SizeMode = PictureBoxSizeMode.StretchImage,
                Name = p.Id.ToString(),
                BackColor = Color.Transparent
            };
            return pic;
        }

        private bool isPlayerPicCreated(int id) {
            if(playerPics[id] != null)
            {
                return true;
            }
            return false;
        }

        private async void leaveBtn_Click(object sender, EventArgs e)
        {
            await removePlayerAsync();
            Close();
        }

        private async void upBtn_Click(object sender, EventArgs e)
        {
            foreach (var item in blocks)
            {
                if (BlockType.Ground.ToString() != item.getType() && item.getPosX() == mePlayer.PosX && item.getPosY() == mePlayer.PosY - 1)
                {
                    return;
                }
            }
            mePlayer.Rotation = 0;
            mePlayer.PosY -= 1;
            await UpdateProductAsync(mePlayer);
            ICollection<Player> playersTemp = await GetAllPlayerAsync(client.BaseAddress.PathAndQuery);
            players = playersTemp.ToList();
        }

        private async void leftBtn_Click(object sender, EventArgs e)
        {
            foreach (var item in blocks)
            {
                if (BlockType.Ground.ToString() != item.getType() && item.getPosX() == mePlayer.PosX - 1 && item.getPosY() == mePlayer.PosY)
                {
                    return;
                }
            }
            mePlayer.Rotation = 1;
            mePlayer.PosX -= 1;
            await UpdateProductAsync(mePlayer);
            ICollection<Player> playersTemp = await GetAllPlayerAsync(client.BaseAddress.PathAndQuery);
            players = playersTemp.ToList();
        }

        private async void downBtn_Click(object sender, EventArgs e)
        {
            foreach (var item in blocks)
            {
                if (BlockType.Ground.ToString() != item.getType() && item.getPosX() == mePlayer.PosX && item.getPosY() == mePlayer.PosY + 1)
                {
                    return;
                }
            }
            mePlayer.Rotation = 2;
            mePlayer.PosY += 1;
            await UpdateProductAsync(mePlayer);
            ICollection<Player> playersTemp = await GetAllPlayerAsync(client.BaseAddress.PathAndQuery);
            players = playersTemp.ToList();
        }

        private async void rightBtn_Click(object sender, EventArgs e)
        {
            foreach (var item in blocks)
            {
                if (BlockType.Ground.ToString() != item.getType() && item.getPosX() == mePlayer.PosX + 1 && item.getPosY() == mePlayer.PosY)
                {
                    return;
                }
            }
            mePlayer.Rotation = 3;
            mePlayer.PosX += 1;
            await UpdateProductAsync(mePlayer);
            ICollection<Player> playersTemp = await GetAllPlayerAsync(client.BaseAddress.PathAndQuery);
            players = playersTemp.ToList();
        }

        //Async tasks
        async Task removePlayerAsync()
        {
            ICollection<Player> playersTemp = await RemovePlayer(client.BaseAddress.PathAndQuery, mePlayer.Id);
            players = playersTemp != null ? playersTemp.ToList() : players;
            mePlayer = null;
        }

        async Task joinGameAsync()
        {
            // Update port # in the following line.
            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            char[] ss = { '[', ']' };
            String stringData = await BuildMap(client.BaseAddress.PathAndQuery);
            List<String> splited = stringData.Split(ss).ToList();
            splited = splited.Where(s => !string.IsNullOrWhiteSpace(s)).Distinct().ToList();
            string jsonCorrect = "";
            string jsonCorrect2 = "";
            string jsonCorrect3 = "";
            int cnt = splited.Count();

            if (cnt != 3)
            {
                return;
            }
            jsonCorrect += "[" + splited[0] + "]";
            var model = JsonConvert.DeserializeObject<List<Brick>>(jsonCorrect);
            foreach (var item in model)
            {
                blocks.Add(item);
            }
            jsonCorrect2 += "[" + splited[1] + "]";
            var modelw = JsonConvert.DeserializeObject<List<Water>>(jsonCorrect2);
            foreach (var item in modelw)
            {
                blocks.Add((Water)item);
            }
            jsonCorrect3 += "[" + splited[2] + "]";
            var model3 = JsonConvert.DeserializeObject<List<Ground>>(jsonCorrect3);
            foreach (var item in model3)
            {
                blocks.Add((Ground)item);
            }

            mePlayer = await createPlayer(client.BaseAddress.PathAndQuery);
            ICollection<Player> playersTemp = await GetAllPlayerAsync(client.BaseAddress.PathAndQuery);
            players = playersTemp.ToList();
            paintMap();
        }

        private void paintMap()
        {
            foreach (var item in blocks)
            {
                if (item != null)
                {
                    if (BlockType.Brick.ToString() == item.getType())
                    {
                        string path = "..\\..\\Images\\brick.jpeg";
                        Image img = Image.FromFile(path);
                        g.DrawImage(img, item.getPosX() * mapSizeX, item.getPosY() * mapSizeX, mapSizeX, mapSizeX);
                    }
                    else if (BlockType.Water.ToString() == item.getType())
                    {
                        string path = "..\\..\\Images\\water.jpg";
                        Image img = Image.FromFile(path);
                        g.DrawImage(img, item.getPosX() * mapSizeX, item.getPosY() * mapSizeX, mapSizeX, mapSizeX);
                    }
                    else if (BlockType.Ground.ToString() == item.getType())
                    {
                        //p.Color = Color.BurlyWood;
                        //sb.Color = Color.BurlyWood;
                        //g.DrawRectangle(p, item.getPosX() * 20, item.getPosY() * mapSizeX, mapSizeX, 20);
                        //g.FillRectangle(sb, (item.getPosX() * mapSizeX) + 1, (item.getPosY() * mapSizeX) + 1, mapSizeX - 1, mapSizeX - 1);
                    }
                    else if (BlockType.Iron.ToString() == item.getType())
                    {
                        //p.Color = Color.Black;
                        //sb.Color = Color.DimGray;
                        //g.DrawRectangle(p, item.getPosX() * mapSizeX, item.getPosY() * mapSizeX, mapSizeX, 20);
                        //g.FillRectangle(sb, (item.getPosX() * mapSizeX) + 1, (item.getPosY() * mapSizeX) + 1, mapSizeX - 1, mapSizeX - 1);
                    }
                    else if (BlockType.Border.ToString() == item.getType())
                    {
                        //p.Color = Color.DarkSlateGray;
                        //sb.Color = Color.Black;
                        //g.DrawRectangle(p, item.getPosX() * mapSizeX, item.getPosY() * mapSizeX, mapSizeX, 20);
                        //g.FillRectangle(sb, (item.getPosX() * mapSizeX) + 1, (item.getPosY() * mapSizeX) + 1, mapSizeX - 1, mapSizeX - 1);
                    }
                }
            }
        }

        private void panel2_Paint(object sender, PaintEventArgs e)
        {
           
        }

        public async Task<Player> createPlayer(string path)
        {
            Player p = null;
            HttpResponseMessage response = await client.PostAsync(path + "api/players/", null);
            if (response.IsSuccessStatusCode)
            {
                p = await response.Content.ReadAsAsync<Player>();
            }
            return p;
        }

        static async Task<ICollection<Player>> GetAllPlayerAsync(string path)
        {
            ICollection<Player> players = null;
            HttpResponseMessage response = await client.GetAsync(path + "api/players");
            if (response.IsSuccessStatusCode)
            {
                players = await response.Content.ReadAsAsync<ICollection<Player>>();
            }
            return players;
        }

        static async Task<ICollection<Player>> RemovePlayer(string path, long playerId)
        {
            ICollection<Player> players = null;
            HttpResponseMessage response = await client.DeleteAsync(path + $"api/players/{playerId}");
            if (response.IsSuccessStatusCode)
            {
                players = await response.Content.ReadAsAsync<ICollection<Player>>();
            }
            return players;
        }

        static async Task<Player> UpdateProductAsync(Player player)
        {
            HttpResponseMessage response = await client.PutAsJsonAsync(
                $"api/players/", player);
            response.EnsureSuccessStatusCode();

            // Deserialize the updated product from the response body.
            player = await response.Content.ReadAsAsync<Player>();
            return player;
        }

        public async Task<string> BuildMap(string path)
        {
            string m = "";
            HttpResponseMessage response = await client.GetAsync(path + "api/map/");
            if (response.IsSuccessStatusCode)
            {
                m = await response.Content.ReadAsAsync<string>();
            }
            return m;
        }

        // PATTERNS

        ///////////////////////////////////        FACTORY                ////////////////////////////////////////////////////////////
        public enum BlockType
        {
            Water,
            Brick,
            Grass,
            Ground,
            Iron,
            Border
        }
        public class BlockFactory
        {
            public IBlock GetBlock(BlockType type)
            {
                switch (type)
                {
                    case BlockType.Brick:
                        return new Brick();
                    case BlockType.Grass:
                        return new Grass();
                    case BlockType.Water:
                        return new Water();
                    case BlockType.Ground:
                        return new Ground();
                    case BlockType.Iron:
                        return new Iron();
                    case BlockType.Border:
                        return new Border();
                    default:
                        throw new NotSupportedException();
                }
            }
        }

        public interface IBlock
        {
            void setPosX(int x);
            void setPosY(int y);
            void setPosXY(int x, int y);
            int getPosX();
            int getPosY();
            string getType();

        }
        public class Brick : IBlock
        {
            public int PosX { get; set; }
            public int PosY { get; set; }
            public string Type { get; } = "Brick";

            public Brick(int PosX, int PosY, string Type)
            {
                this.PosX = PosX;
                this.PosY = PosY;
                this.Type = Type;
            }
            public Brick()
            {

            }

            public int getPosX()
            {
                return PosX;
            }

            public int getPosY()
            {
                return PosY;
            }

            public void setPosX(int x)
            {
                PosX = x;
            }

            public void setPosY(int y)
            {
                PosY = y;
            }
            public void setPosXY(int x, int y)
            {
                PosY = y;
                PosX = x;
            }


            public string getType()
            {
                return Type;
            }
        }
        public class Iron : IBlock
        {
            public int PosX { get; set; }
            public int PosY { get; set; }
            public string Type { get; } = "Iron";

            public Iron(int PosX, int PosY, string Type)
            {
                this.PosX = PosX;
                this.PosY = PosY;
                this.Type = Type;
            }
            public Iron()
            {

            }

            public int getPosX()
            {
                return PosX;
            }

            public int getPosY()
            {
                return PosY;
            }

            public void setPosX(int x)
            {
                PosX = x;
            }

            public void setPosY(int y)
            {
                PosY = y;
            }
            public void setPosXY(int x, int y)
            {
                PosY = y;
                PosX = x;
            }


            public string getType()
            {
                return Type;
            }
        }
        public class Border : IBlock
        {
            public int PosX { get; set; }
            public int PosY { get; set; }
            public string Type { get; } = "Border";

            public Border(int PosX, int PosY, string Type)
            {
                this.PosX = PosX;
                this.PosY = PosY;
                this.Type = Type;
            }
            public Border()
            {

            }

            public int getPosX()
            {
                return PosX;
            }

            public int getPosY()
            {
                return PosY;
            }

            public void setPosX(int x)
            {
                PosX = x;
            }

            public void setPosY(int y)
            {
                PosY = y;
            }
            public void setPosXY(int x, int y)
            {
                PosY = y;
                PosX = x;
            }


            public string getType()
            {
                return Type;
            }
        }
        public class Ground : IBlock
        {
            public int PosX { get; set; }
            public int PosY { get; set; }
            public string Type { get; } = "Ground";

            public Ground(int PosX, int PosY, string Type)
            {
                this.PosX = PosX;
                this.PosY = PosY;
                this.Type = Type;
            }
            public Ground()
            {

            }

            public int getPosX()
            {
                return PosX;
            }

            public int getPosY()
            {
                return PosY;
            }

            public void setPosX(int x)
            {
                PosX = x;
            }

            public void setPosY(int y)
            {

            }

            public string getType()
            {
                return Type;
            }

            public void setPosXY(int x, int y)
            {
                PosX = x;
                PosY = y;
            }
        }
        public class Water : IBlock
        {
            public int PosX { get; set; }
            public int PosY { get; set; }

            public string Type { get; } = "Water";


            public Water(int PosX, int PosY, string Type)
            {
                this.PosX = PosX;
                this.PosY = PosY;
                this.Type = Type;
            }
            public Water()
            {

            }

            public int getPosX()
            {
                return PosX;
            }

            public int getPosY()
            {
                return PosY;
            }

            public void setPosX(int x)
            {
                PosX = x;
            }
            public void setPosXY(int x, int y)
            {
                PosX = x;
                PosY = y;
            }
            public void setPosY(int y)
            {
                PosY = y;
            }
            public string getType()
            {
                return Type;
            }
        }
        public class Grass : IBlock
        {
            public int PosX { get; set; }
            public int PosY { get; set; }

            public string Type { get; } = "Grass";

            public Grass(int PosX, int PosY, string Type)
            {
                this.PosX = PosX;
                this.PosY = PosY;
                this.Type = Type;
            }
            public Grass()
            {

            }
            public void setPosXY(int x, int y)
            {
                PosX = x;
                PosY = y;
            }
            public int getPosX()
            {
                return PosX;
            }

            public int getPosY()
            {
                return PosY;
            }

            public void setPosX(int x)
            {
                PosX = x;
            }

            public void setPosY(int y)
            {
                PosY = y;
            }
            public string getType()
            {
                return Type;
            }



        }
        /////////////////////////////////////// ABSTRACT FACTORY /////////////////////////////////////////


        public abstract class ItemFactory
        {
            public abstract IStatusPowerUp createStatusPowerUp();
            public abstract IConsumablePowerUp createConsumablePowerUp();
        }

        public class OffensiveItemFactory : ItemFactory
        {
            public override IConsumablePowerUp createConsumablePowerUp()
            {
                return new Mine();
            }

            public override IStatusPowerUp createStatusPowerUp()
            {
                return new DamagePowerUp();
            }
        }
        public class DefensiveItemFactory : ItemFactory
        {
            public override IConsumablePowerUp createConsumablePowerUp()
            {
                return new Shield();
            }

            public override IStatusPowerUp createStatusPowerUp()
            {
                return new HealthPowerUp();
            }
        }

        public interface IPickUpItem
        {
            void setPosX(int x);
            void setPosY(int y);
            int getPosX();
            int getPosY();
        }
        public interface IStatusPowerUp : IPickUpItem
        {
            void increaseStats();
        }
        public interface IConsumablePowerUp : IPickUpItem
        {
            void activateConsumable();
        }

        public class Shield : IConsumablePowerUp
        {

            private int PosX { get; set; }
            private int PosY { get; set; }


            public Shield(int PosX, int PosY)
            {
                this.PosX = PosX;
                this.PosY = PosY;
            }
            public Shield()
            {

            }


            public int getPosX()
            {
                return PosX;
            }

            public int getPosY()
            {
                return PosY;
            }

            public void setPosX(int x)
            {
                PosX = x;
            }

            public void setPosY(int y)
            {
                PosY = y;
            }

            public void activateConsumable()
            {
                throw new NotImplementedException();
            }
        }

        public class Mine : IConsumablePowerUp
        {

            private int PosX { get; set; }
            private int PosY { get; set; }


            public Mine(int PosX, int PosY)
            {
                this.PosX = PosX;
                this.PosY = PosY;
            }
            public Mine()
            {

            }

            public int getPosX()
            {
                return PosX;
            }

            public int getPosY()
            {
                return PosY;
            }

            public void setPosX(int x)
            {
                PosX = x;
            }

            public void setPosY(int y)
            {
                PosY = y;
            }

            public void activateConsumable()
            {
                throw new NotImplementedException();
            }
        }


        public class DamagePowerUp : IStatusPowerUp
        {

            private int PosX { get; set; }
            private int PosY { get; set; }


            public DamagePowerUp(int PosX, int PosY)
            {
                this.PosX = PosX;
                this.PosY = PosY;
            }
            public DamagePowerUp()
            {

            }
            public void increaseStats()
            {
                throw new NotImplementedException();
            }

            public int getPosX()
            {
                return PosX;
            }

            public int getPosY()
            {
                return PosY;
            }

            public void setPosX(int x)
            {
                PosX = x;
            }

            public void setPosY(int y)
            {
                PosY = y;
            }
        }
        public class HealthPowerUp : IStatusPowerUp
        {
            private int PosX { get; set; }
            private int PosY { get; set; }
            public HealthPowerUp(int PosX, int PosY)
            {
                this.PosX = PosX;
                this.PosY = PosY;
            }
            public HealthPowerUp()
            {

            }
            public void increaseStats()
            {
                throw new NotImplementedException();
            }

            public int getPosX()
            {
                return PosX;
            }

            public int getPosY()
            {
                return PosY;
            }

            public void setPosX(int x)
            {
                PosX = x;
            }

            public void setPosY(int y)
            {
                PosY = y;
            }
        }

        /// <summary>
        /// /////////////////////////////////////    STRATEGY //////////////////////////////////////////
        /// </summary>
        public interface IMoveAlgorithm
        {
            void changeDirection(Player p);
        }

        public class MoveUp : IMoveAlgorithm
        {
            public void changeDirection(Player p)
            {
                if (p.getPosY() != 100)
                {
                    p.addPosY(1);
                }
            }

        }
        public class MoveDown : IMoveAlgorithm
        {
            public void changeDirection(Player p)
            {
                if (p.getPosY() != 0)
                {
                    p.addPosY(-1);
                }
            }
        }


        public class MoveLeft : IMoveAlgorithm
        {
            public void changeDirection(Player p)
            {
                if (p.getPosX() != 0)
                {
                    p.addPosX(-1);
                }
            }

        }
        public class MoveRight : IMoveAlgorithm
        {
            public void changeDirection(Player p)
            {
                if (p.getPosX() != 100)
                {
                    p.addPosY(1);
                }
            }
        }

        public class Player
        {

            public int Id { get; set; }
            public int PosX { get; set; }
            public int PosY { get; set; }
            public int Speed { get; set; }
            public int Rotation { get; set; }

            public Player(int posX, int posY, int speed)
            {
                PosX = posX;
                PosY = posY;
                Speed = speed;
            }
            public Player()
            {

            }
            public void setPosX(int posX)
            {
                PosX = posX;
            }
            public void setPosY(int posY)
            {
                PosY = posY;
            }
            public int getPosY()
            {
                return PosY;
            }
            public int getPosX()
            {
                return PosX;
            }
            public void addPosY(int x)
            {
                PosY += x * Speed;
            }
            public void addPosX(int x)
            {
                PosX += x * Speed;
            }

            public IMoveAlgorithm direction;
            public Player(IMoveAlgorithm iMoveAlgorithm)
            {
                this.direction = iMoveAlgorithm;
            }
            public void changeDirection()
            {
                direction.changeDirection(this);
            }
        }

        ////////////////////// BUILDER //////////////////////////

        public class MapDirector
        {
            public Map BuildMap(MapBuilder mapBuilder)
            {
                mapBuilder.AddBlocks();
                mapBuilder.AddItems();

                return mapBuilder.Map;
            }
        }

        public class Map
        {
            public List<IPickUpItem> Items { get; set; } = new List<IPickUpItem>();

            //    public List<IBlock> Blocks { get; set; } = new List<IBlock>();

            public IBlock[,] Blocks { get; set; }


            public int SizeX { get; set; }

            public int SizeY { get; set; }

            public Map(int SizeX, int SizeY)
            {
                this.SizeX = SizeX;
                this.SizeY = SizeY;
            }
            public Map()
            {

            }

            public void CreateBlocksArray()
            {
                Blocks = new IBlock[SizeX, SizeY];
            }

        }
        public abstract class MapBuilder
        {
            public BlockFactory BlockFactory = new BlockFactory();

            public ItemFactory Itemfactory;


            public Map Map { get; private set; }
            public void CreateMap(int SizeX, int SizeY)
            {
                Map = new Map(SizeX, SizeY);
                Map.CreateBlocksArray();
            }

            public abstract void AddBlocks();
            public abstract void AddItems();
        }
        public class SmalMapBuilder : MapBuilder
        {
            public override void AddBlocks()
            {// Tik kaip pvz. Reikia sugalvoti algoritma kuris sugeneruos visus blokus zemelapije.
                for (int i = 0; i < Map.SizeX; i++)
                {
                    this.Map.Blocks[i, 0] = BlockFactory.GetBlock(BlockType.Border);
                    this.Map.Blocks[i, 0].setPosXY(i, 0);

                    this.Map.Blocks[0, i] = BlockFactory.GetBlock(BlockType.Border);
                    this.Map.Blocks[0, i].setPosXY(0, i);

                    this.Map.Blocks[Map.SizeX - 1, i] = BlockFactory.GetBlock(BlockType.Border);
                    this.Map.Blocks[Map.SizeX - 1, i].setPosXY(Map.SizeX - 1, i);

                    this.Map.Blocks[i, Map.SizeY - 1] = BlockFactory.GetBlock(BlockType.Border);
                    this.Map.Blocks[i, Map.SizeY - 1].setPosXY(i, Map.SizeY - 1);

                }
                Random rnd = new Random();
                int waterCnt = rnd.Next(1, 3);

                for (int x = 0; x < waterCnt; x++)
                {
                    int x1 = rnd.Next(1, 19);
                    int x2 = rnd.Next(1, 19);
                    int y1 = rnd.Next(1, 19);
                    int y2 = rnd.Next(1, 19);

                    this.Map.Blocks[x1, y1] = BlockFactory.GetBlock(BlockType.Water);
                    this.Map.Blocks[x1, y1].setPosXY(x1, y1);

                    this.Map.Blocks[x2, y2] = BlockFactory.GetBlock(BlockType.Water);
                    this.Map.Blocks[x2, y2].setPosXY(x2, y2);

                    int numOfBlocksTodrawX = 0;
                    int numOfBlocksTodrawY = 0;
                    int tempX = 0;

                    if (x1 < x2)
                    {
                        numOfBlocksTodrawX = x2 - x1;
                        for (int i = 1; i < numOfBlocksTodrawX + 1; i++)
                        {
                            if (this.Map.Blocks[x1 + i, y1] == null)
                            {
                                this.Map.Blocks[x1 + i, y1] = BlockFactory.GetBlock(BlockType.Water);
                                this.Map.Blocks[x1 + i, y1].setPosXY(x1 + i, y1);
                            }
                        }
                        tempX = x2;
                    }
                    else
                    {
                        numOfBlocksTodrawX = x1 - x2;
                        for (int i = 1; i < numOfBlocksTodrawX + 1; i++)
                        {
                            if (this.Map.Blocks[x2 + i, y1] == null)
                            {
                                this.Map.Blocks[x2 + i, y2] = BlockFactory.GetBlock(BlockType.Water);
                                this.Map.Blocks[x2 + i, y2].setPosXY(x2 + i, y2);
                            }
                        }
                        tempX = x1;
                    }

                    if (y1 < y2)
                    {
                        numOfBlocksTodrawY = y2 - y1;
                        for (int i = 1; i < numOfBlocksTodrawY; i++)
                        {
                            if (this.Map.Blocks[tempX, y1 + i] == null)
                            {
                                this.Map.Blocks[tempX, y1 + i] = BlockFactory.GetBlock(BlockType.Water);
                                this.Map.Blocks[tempX, y1 + i].setPosXY(tempX, y1 + i);
                            }
                        }
                    }
                    else
                    {
                        numOfBlocksTodrawY = y1 - y2;
                        for (int i = 1; i < numOfBlocksTodrawY; i++)
                        {
                            if (this.Map.Blocks[tempX, y2 + i] == null)
                            {
                                this.Map.Blocks[tempX, y2 + i] = BlockFactory.GetBlock(BlockType.Water);
                                this.Map.Blocks[tempX, y2 + i].setPosXY(tempX, y2 + i);
                            }
                        }
                    }
                }
                int bricksPatter = rnd.Next(0, 2);

                if (bricksPatter == 0)
                {
                    for (int i = 2; i < Map.SizeX - 2; i++)
                    {
                        this.Map.Blocks[i, i] = BlockFactory.GetBlock(BlockType.Brick);
                        this.Map.Blocks[i, i].setPosXY(i, i);

                        this.Map.Blocks[Map.SizeX - i, i] = BlockFactory.GetBlock(BlockType.Brick);
                        this.Map.Blocks[Map.SizeX - i, i].setPosXY(Map.SizeX - i, i);

                    }

                }
                if (bricksPatter == 1)
                {
                    for (int i = Map.SizeX / 5; i < (Map.SizeX - Map.SizeX / 5) + 1; i++)
                    {
                        this.Map.Blocks[i, Map.SizeX / 5] = BlockFactory.GetBlock(BlockType.Brick);
                        this.Map.Blocks[i, Map.SizeX / 5].setPosXY(i, Map.SizeX / 5);

                        this.Map.Blocks[Map.SizeX / 5, i] = BlockFactory.GetBlock(BlockType.Brick);
                        this.Map.Blocks[Map.SizeX / 5, i].setPosXY(Map.SizeX / 5, i);

                        this.Map.Blocks[i, Map.SizeX - Map.SizeX / 5] = BlockFactory.GetBlock(BlockType.Brick);
                        this.Map.Blocks[i, Map.SizeX - Map.SizeX / 5].setPosXY(i, Map.SizeX - Map.SizeX / 5);


                        this.Map.Blocks[Map.SizeX - Map.SizeX / 5, i] = BlockFactory.GetBlock(BlockType.Brick);
                        this.Map.Blocks[Map.SizeX - Map.SizeX / 5, i].setPosXY(Map.SizeX - Map.SizeX / 5, i);

                    }


                    for (int i = 1; i < Map.SizeX - 1; i++)
                    {

                        this.Map.Blocks[i, Map.SizeX - Map.SizeX / 2] = BlockFactory.GetBlock(BlockType.Brick);
                        this.Map.Blocks[i, Map.SizeX - Map.SizeX / 2].setPosXY(i, Map.SizeX - Map.SizeX / 2);


                        this.Map.Blocks[Map.SizeX - Map.SizeX / 2, i] = BlockFactory.GetBlock(BlockType.Brick);
                        this.Map.Blocks[Map.SizeX - Map.SizeX / 2, i].setPosXY(Map.SizeX - Map.SizeX / 2, i);
                    }
                }


                for (int i = 0; i < Map.SizeX; i++)
                {
                    for (int j = 0; j < Map.SizeY; j++)
                    {
                        if (Map.Blocks[i, j] == null)
                        {
                            Map.Blocks[i, j] = BlockFactory.GetBlock(BlockType.Ground);
                            Map.Blocks[i, j].setPosXY(i, j);
                        }
                    }
                }

            }

            public override void AddItems()
            {
                Itemfactory = new DefensiveItemFactory();
                this.Map.Items.Add(Itemfactory.createStatusPowerUp());
                //throw new NotImplementedException();
            }

        }
        public class LargeMapBuilder : MapBuilder
        {
            public override void AddBlocks()
            {

                throw new NotImplementedException();
            }

            public override void AddItems()
            {

                throw new NotImplementedException();
            }
        }
    }
}
