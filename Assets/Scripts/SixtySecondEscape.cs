using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class SixtySecondEscape : MonoBehaviour
{
    enum State { Menu, Playing, Paused, GameOver, Won }
    State state = State.Menu;

    const float ROUND_TIME = 60f;
    const float LANE = 2.35f;
    const float START_Z = 34f;
    const float DESPAWN_Z = -15f;

    GameObject player, world;
    Camera cam;
    Canvas canvas;
    Text title, timerText, scoreText, coinText, comboText, powerText;
    Button playButton, pauseButton, resumeButton, restartButton;
    float timeLeft, score, speed, spawnTimer, comboTimer, shieldTimer, slowTimer;
    int coins, combo;
    bool shield;
    readonly List<GameObject> spawned = new List<GameObject>();

    Material Mat(Color c)
    {
        var m = new Material(Shader.Find("Standard"));
        m.color = c;
        return m;
    }

    GameObject Cube(string name, Vector3 pos, Vector3 scale, Color color)
    {
        var g = GameObject.CreatePrimitive(PrimitiveType.Cube);
        g.name = name; g.transform.position = pos; g.transform.localScale = scale;
        g.GetComponent<Renderer>().material = Mat(color);
        return g;
    }

    void Start()
    {
        Application.targetFrameRate = 60;
        BuildWorld();
        BuildUI();
        ShowMenu();
    }

    void BuildWorld()
    {
        world = new GameObject("World");
        Cube("Road", new Vector3(0,-.35f,35), new Vector3(8,.5f,110), new Color(.035f,.045f,.07f)).transform.SetParent(world.transform);
        Cube("LeftNeon", new Vector3(-4.7f,.05f,35), new Vector3(.18f,.5f,110), new Color(.1f,.8f,1f)).transform.SetParent(world.transform);
        Cube("RightNeon", new Vector3(4.7f,.05f,35), new Vector3(.18f,.5f,110), new Color(1f,.15f,.55f)).transform.SetParent(world.transform);

        player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        player.name = "Player";
        player.transform.position = new Vector3(0,1.1f,0);
        player.transform.localScale = new Vector3(.75f,1.05f,.75f);
        player.GetComponent<Renderer>().material = Mat(new Color(.05f,.85f,1f));
        player.AddComponent<PlayerHit>().game = this;

        var light = new GameObject("PlayerLight").AddComponent<Light>();
        light.type = LightType.Point; light.range = 7; light.intensity = 4;
        light.transform.SetParent(player.transform);
        light.transform.localPosition = new Vector3(0,1,0);

        cam = new GameObject("Main Camera").AddComponent<Camera>();
        cam.transform.position = new Vector3(0,5.5f,-8);
        cam.transform.rotation = Quaternion.Euler(18,0,0);
        cam.fieldOfView = 65;
    }

    Font Font() => Resources.GetBuiltinResource<Font>("Arial.ttf");

    Text AddText(Transform parent, string text, int size, Vector2 pos, Vector2 dim)
    {
        var go = new GameObject("Text");
        go.transform.SetParent(parent);
        var t = go.AddComponent<Text>();
        t.text=text; t.font=Font(); t.fontSize=size; t.alignment=TextAnchor.MiddleCenter; t.color=Color.white;
        var r=t.rectTransform; r.sizeDelta=dim; r.anchoredPosition=pos;
        return t;
    }

    Button AddButton(Transform parent, string label, Vector2 pos)
    {
        var b=new GameObject(label).AddComponent<Button>();
        b.transform.SetParent(parent);
        var r=b.GetComponent<RectTransform>(); r.sizeDelta=new Vector2(620,150); r.anchoredPosition=pos;
        var img=b.gameObject.AddComponent<Image>(); img.color=new Color(.05f,.35f,.65f,.95f);
        AddText(b.transform,label,46,Vector2.zero,new Vector2(620,150));
        return b;
    }

    void BuildUI()
    {
        var go=new GameObject("Canvas");
        canvas=go.AddComponent<Canvas>(); canvas.renderMode=RenderMode.ScreenSpaceOverlay;
        go.AddComponent<CanvasScaler>().uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;
        go.GetComponent<CanvasScaler>().referenceResolution=new Vector2(1080,1920);
        go.AddComponent<GraphicRaycaster>();

        title=AddText(canvas.transform,"60 SECOND ESCAPE",72,new Vector2(0,520),new Vector2(1000,180));
        timerText=AddText(canvas.transform,"60.0",72,new Vector2(0,760),new Vector2(500,130));
        scoreText=AddText(canvas.transform,"SCORE 0",40,new Vector2(-330,820),new Vector2(500,100));
        coinText=AddText(canvas.transform,"COINS 0",40,new Vector2(330,820),new Vector2(500,100));
        comboText=AddText(canvas.transform,"",38,new Vector2(0,620),new Vector2(700,90));
        powerText=AddText(canvas.transform,"",34,new Vector2(0,-700),new Vector2(900,100));

        playButton=AddButton(canvas.transform,"PLAY",new Vector2(0,150));
        restartButton=AddButton(canvas.transform,"PLAY AGAIN",new Vector2(0,-250));
        resumeButton=AddButton(canvas.transform,"RESUME",new Vector2(0,0));
        pauseButton=AddButton(canvas.transform,"Ⅱ",new Vector2(450,820));
        playButton.onClick.AddListener(StartRound);
        restartButton.onClick.AddListener(StartRound);
        resumeButton.onClick.AddListener(Resume);
        pauseButton.onClick.AddListener(Pause);
    }

    void Hide(Button b){ if(b) b.gameObject.SetActive(false); }

    void ShowMenu()
    {
        state=State.Menu;
        title.text="60 SECOND ESCAPE\nSURVIVE THE NEON RUN";
        title.gameObject.SetActive(true);
        playButton.gameObject.SetActive(true);
        restartButton.gameObject.SetActive(false);
        resumeButton.gameObject.SetActive(false);
        pauseButton.gameObject.SetActive(false);
        timerText.gameObject.SetActive(false);
        scoreText.gameObject.SetActive(false);
        coinText.gameObject.SetActive(false);
        comboText.gameObject.SetActive(false);
        powerText.gameObject.SetActive(false);
    }

    void StartRound()
    {
        foreach(var g in spawned) if(g) Destroy(g);
        spawned.Clear();
        player.transform.position=new Vector3(0,1.1f,0);
        timeLeft=ROUND_TIME; score=0; coins=0; combo=0; comboTimer=0;
        shield=false; shieldTimer=0; slowTimer=0; speed=9; spawnTimer=.2f;
        state=State.Playing;
        title.gameObject.SetActive(false); playButton.gameObject.SetActive(false);
        restartButton.gameObject.SetActive(false); resumeButton.gameObject.SetActive(false);
        pauseButton.gameObject.SetActive(true);
        timerText.gameObject.SetActive(true); scoreText.gameObject.SetActive(true);
        coinText.gameObject.SetActive(true); comboText.gameObject.SetActive(true); powerText.gameObject.SetActive(true);
    }

    void Update()
    {
        if(state==State.Playing) Tick();
        if(state==State.Playing || state==State.Paused)
            cam.transform.position=Vector3.Lerp(cam.transform.position,new Vector3(player.transform.position.x*.25f,5.5f,-8),Time.deltaTime*5);
    }

    void Tick()
    {
        float dt=Time.deltaTime;
        HandleInput();
        timeLeft-=dt;
        speed=9f+(ROUND_TIME-timeLeft)*.16f;
        if(slowTimer>0) slowTimer-=dt;
        if(shieldTimer>0){ shieldTimer-=dt; if(shieldTimer<=0) shield=false; }
        comboTimer-=dt; if(comboTimer<=0) combo=0;

        score += dt*100*(1+combo*.1f);
        spawnTimer-=dt;
        if(spawnTimer<=0){ SpawnWave(); spawnTimer=Mathf.Max(.38f,1.0f-(ROUND_TIME-timeLeft)*.009f); }

        for(int i=spawned.Count-1;i>=0;i--){
            var g=spawned[i]; if(!g){spawned.RemoveAt(i);continue;}
            g.transform.Translate(Vector3.back*speed*(slowTimer>0?.55f:1f)*dt,Space.World);
            if(g.transform.position.z<DESPAWN_Z){Destroy(g);spawned.RemoveAt(i);}
        }

        timerText.text=timeLeft.ToString("0.0");
        scoreText.text="SCORE "+Mathf.FloorToInt(score);
        coinText.text="COINS "+coins;
        comboText.text=combo>1?"COMBO x"+combo:"";
        powerText.text=shieldTimer>0?"🛡 SHIELD "+shieldTimer.ToString("0.0"):
                         slowTimer>0?"⏱ SLOW-MO "+slowTimer.ToString("0.0"):"";

        if(timeLeft<=0) Win();
    }

    void HandleInput()
    {
        float move=0;
        if(Input.GetKey(KeyCode.LeftArrow)||Input.GetKey(KeyCode.A))move=-1;
        if(Input.GetKey(KeyCode.RightArrow)||Input.GetKey(KeyCode.D))move=1;
        if(Input.touchCount>0){
            var t=Input.GetTouch(0);
            if(t.phase==TouchPhase.Moved) move=Mathf.Abs(t.deltaPosition.x)>5?Mathf.Sign(t.deltaPosition.x):0;
        }
        player.transform.position+=Vector3.right*move*7f*Time.deltaTime;
        player.transform.position=new Vector3(Mathf.Clamp(player.transform.position.x,-3.45f,3.45f),1.1f,0);
    }

    void SpawnWave()
    {
        int lane=Random.Range(-1,2);
        int roll=Random.Range(0,100);
        float z=START_Z+Random.Range(0,10);
        if(roll<55){
            var o=Cube("Obstacle",new Vector3(lane*LANE,1,z),new Vector3(1.65f,2f,1.65f),new Color(1f,.12f,.12f));
            o.AddComponent<Obstacle>().game=this; spawned.Add(o);
        } else if(roll<78){
            var c=GameObject.CreatePrimitive(PrimitiveType.Sphere);
            c.name="Coin"; c.transform.position=new Vector3(lane*LANE,1.3f,z); c.transform.localScale=Vector3.one*.55f;
            c.GetComponent<Renderer>().material=Mat(new Color(1f,.75f,.05f)); c.GetComponent<Collider>().isTrigger=true;
            c.AddComponent<Coin>().game=this; spawned.Add(c);
        } else {
            var p=GameObject.CreatePrimitive(PrimitiveType.Sphere);
            p.name="PowerUp"; p.transform.position=new Vector3(lane*LANE,1.3f,z); p.transform.localScale=Vector3.one*.8f;
            p.GetComponent<Renderer>().material=Mat(roll<89?new Color(.2f,1f,.4f):new Color(.7f,.3f,1f));
            p.GetComponent<Collider>().isTrigger=true; p.AddComponent<PowerUp>().game=this; spawned.Add(p);
        }
    }

    public void CollectCoin(GameObject g)
    {
        coins++; combo++; comboTimer=2.5f; score+=250*(1+combo);
        spawned.Remove(g); Destroy(g);
    }

    public void CollectPower(GameObject g)
    {
        if(Random.value>.5f){ shield=true; shieldTimer=7; }
        else slowTimer=6;
        score+=500; spawned.Remove(g); Destroy(g);
    }

    public void Hit()
    {
        if(state!=State.Playing)return;
        if(shield){ shield=false; shieldTimer=0; return; }
        state=State.GameOver;
        title.text="GAME OVER\nSCORE "+Mathf.FloorToInt(score);
        title.gameObject.SetActive(true); restartButton.gameObject.SetActive(true); pauseButton.gameObject.SetActive(false);
    }

    void Win()
    {
        if(state!=State.Playing)return;
        state=State.Won; score+=5000;
        title.text="YOU SURVIVED!\n+5000 BONUS\nSCORE "+Mathf.FloorToInt(score);
        title.gameObject.SetActive(true); restartButton.gameObject.SetActive(true); pauseButton.gameObject.SetActive(false);
    }

    void Pause()
    {
        if(state!=State.Playing)return;
        state=State.Paused; Time.timeScale=0; resumeButton.gameObject.SetActive(true); pauseButton.gameObject.SetActive(false);
    }

    void Resume()
    {
        Time.timeScale=1; state=State.Playing; resumeButton.gameObject.SetActive(false); pauseButton.gameObject.SetActive(true);
    }

    void OnApplicationPause(bool paused)
    {
        if(paused && state==State.Playing) Pause();
    }
}

public class PlayerHit : MonoBehaviour
{
    public SixtySecondEscape game;
    void OnCollisionEnter(Collision c){if(c.gameObject.GetComponent<Obstacle>())game.Hit();}
    void OnTriggerEnter(Collider c){
        var coin=c.GetComponent<Coin>(); if(coin)coin.Collect();
        var p=c.GetComponent<PowerUp>(); if(p)p.Collect();
    }
}
public class Obstacle:MonoBehaviour{public SixtySecondEscape game;}
public class Coin:MonoBehaviour{
    public SixtySecondEscape game;
    public void Collect(){game.CollectCoin(gameObject);}
}
public class PowerUp:MonoBehaviour{
    public SixtySecondEscape game;
    public void Collect(){game.CollectPower(gameObject);}
}
