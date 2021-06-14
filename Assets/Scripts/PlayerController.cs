using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Experimental.Rendering.Universal;

public enum Direction
{
    UP = 0, RIGHT = 1, DOWN = 2, LEFT = 3, UNKNOWN = 4
}

public class PlayerController : MonoBehaviour
{
    public float MoveSpeed = 5f;
    public Transform MovePoint;
    public LayerMask ObstacleMask;
    // public Transform DashShadow;

    public GameObject GoodTile;
    public GameObject BadTile;
    public LayerMask GoodMask;
    public LayerMask BadMask;
    public LayerMask GoalLayer;
    public LayerMask CastableLayer;

    public Transform GoodTileHolder;
    public Transform BadTileHolder;
    public Material OutlineShaderGood;
    public Material OutlineShaderBad;

    public GameObject TrailGood;
    public GameObject TrailBad;
    public GameObject Goal;
    public Sprite GoalOpen;
    public Sprite GoalClosed;

    public Sprite CharacterBad;
    public Sprite CharacterGood;

    public Sprite CrystalGood;
    public Sprite SolidGood;

    public Sprite CrystalBad;
    public Sprite SolidBad;

    public int RequiredKey = 0;
    private int _heldKey = 0;

    // private List<Transform> _shadows;

    [SerializeField] private Vector2 _initialPos;
    private SpriteRenderer _renderer;
    private Animator _anim;
    public Volume GlobalVolume;
    private Bloom bloom;
    public RecordHolder holder;

    private float _moveH;
    private float _moveV;
    private Rigidbody2D _rb2D;
    private int _currInd = 0;

    public Direction _currentDirection = Direction.UNKNOWN;

    [SerializeField] private bool _isSliding = false;
    [SerializeField] private Vector3 dir;
    [SerializeField] private float _moveSmooth;

    private Vector2 prevVel = Vector2.zero;

    private void Start()
    {
        // _shadows = new List<Transform>();
        MovePoint.parent = null;
        _rb2D = GetComponent<Rigidbody2D>();
        //foreach (Transform child in DashShadow)
        //{
        //_shadows.Add(child);
        //}
        holder = GameObject.Find("RecordHolder").GetComponent<RecordHolder>();
        _renderer = GetComponent<SpriteRenderer>();
        _anim = GetComponent<Animator>();
        _initialPos = transform.position;
        // GlobalVolume = GameObject.Find("Global Volume").GetComponent<Volume>();
        GlobalVolume.profile.TryGet(out bloom);
        if (GameManager_.Instance.GoodSide) SetScreenToGood();
        else SetScreenToBad();
        if (_heldKey < RequiredKey) Goal.GetComponent<SpriteRenderer>().sprite = GoalClosed;
    }

    private void Update()
    {
        if (_heldKey < RequiredKey) Goal.GetComponent<SpriteRenderer>().sprite = GoalClosed;
        else Goal.GetComponent<SpriteRenderer>().sprite = GoalOpen;
        //_rb2D.MovePosition(MovePoint.position);

        _moveH = Input.GetAxisRaw("Horizontal");
        _moveV = Input.GetAxisRaw("Vertical");
    }

    public void ChangeSide(bool state)
    {
        if (state == true) // good
        {
            int badtile = 1 << LayerMask.NameToLayer("BadTile");
            int obstacle = 1 << LayerMask.NameToLayer("Obstacle");
            ObstacleMask = badtile | obstacle;
        } else
        {
            int goodtile = 1 << LayerMask.NameToLayer("GoodTile");
            int obstacle = 1 << LayerMask.NameToLayer("Obstacle");
            ObstacleMask = goodtile | obstacle;
        }

    }

    public void SetScreenToBad()
    {
        foreach (Transform child in GoodTileHolder)
        {
            child.GetComponent<Light2D>().enabled = true;
            child.GetComponent<SpriteRenderer>().sprite = SolidGood;
        }
        foreach (Transform child in BadTileHolder)
        {
            child.GetComponent<Light2D>().enabled = false;
            child.GetComponent<SpriteRenderer>().sprite = CrystalBad;
        }
        GameManager_.Instance.GoodSide = false;
        ChangeSide(false);
        // print(OutlineShader.GetColor("_Color"));
        _renderer.material = OutlineShaderBad;
        TrailBad.SetActive(true);
        TrailGood.SetActive(false);
        _renderer.sprite = CharacterBad;
        bloom.tint.Override(new Color(1, 0, 1, 1));
        holder.audio.PlayOneShot(holder.audioList[4]);
        holder.PlayDark();
        // Goal.GetComponent<SpriteRenderer>().sprite = GoalBad;
        // Goal.GetComponent<SpriteRenderer>().material = OutlineShaderBad;
        _anim.Play("CharacterIdleBad");
        // _initialPos = transform.position;
    }

    public void SetScreenToGood()
    {
        foreach (Transform child in GoodTileHolder)
        {
            child.GetComponent<Light2D>().enabled = false;
            child.GetComponent<SpriteRenderer>().sprite = CrystalGood;
        }
        foreach (Transform child in BadTileHolder)
        {
            child.GetComponent<Light2D>().enabled = true;
            child.GetComponent<SpriteRenderer>().sprite = SolidBad;
        }
        GameManager_.Instance.GoodSide = true;
        ChangeSide(true);
        _renderer.material = OutlineShaderGood;
        TrailBad.SetActive(false);
        TrailGood.SetActive(true);
        // Goal.GetComponent<SpriteRenderer>().sprite = GoalGood;
        // Goal.GetComponent<SpriteRenderer>().material = OutlineShaderGood;
        holder.audio.PlayOneShot(holder.audioList[3]);
        holder.PlayLight();
        bloom.tint.Override(new Color(1, 1, 0, 1));
        _renderer.sprite = CharacterGood;
        _anim.Play("CharacterIdle");
        // _initialPos = transform.position;
    }

   
    private void FixedUpdate()
    {
        if (GameManager_.Instance.IsRunningGame == false) return;
        bool prevState = _isSliding;
        if (_currentDirection == Direction.UNKNOWN)
        {
            if (_moveH > 0 && !Physics2D.Raycast(transform.position, Vector3.right, 0.5f, ObstacleMask)) _currentDirection = Direction.RIGHT;
            else if (_moveH < 0 && !Physics2D.Raycast(transform.position, Vector3.left, 0.5f, ObstacleMask)) _currentDirection = Direction.LEFT;
            else if (_moveV > 0 && !Physics2D.Raycast(transform.position, Vector3.up, 0.5f, ObstacleMask)) _currentDirection = Direction.UP;
            else if (_moveV < 0 && !Physics2D.Raycast(transform.position, Vector3.down, 0.5f, ObstacleMask)) _currentDirection = Direction.DOWN;
        }

        // --- MOVING RIGHT --- //
        if (_currentDirection == Direction.RIGHT)
        {
            // if (!Physics2D.Raycast(transform.position, Vector3.right, 0.5f, ObstacleMask))
            if (!Physics2D.Raycast(transform.position, Vector3.right, 0.5f, ObstacleMask))
            {
                dir = Vector3.right * 1;
                _renderer.flipX = false;
                _currentDirection = Direction.RIGHT;
                StartMoving();
            }
            else
            {
                StopMoving();
            }
        }

        // --- MOVING LEFT --- //
        else if (_currentDirection == Direction.LEFT)
        {
            if (!Physics2D.Raycast(transform.position, Vector3.left, 0.5f, ObstacleMask))
            {
                dir = Vector3.right * -1;
                _currentDirection = Direction.LEFT;
                _renderer.flipX = true;
                StartMoving();
            }
            else
            {
                StopMoving();
            }
        }

        // --- MOVING UP --- //
        else if (_currentDirection == Direction.UP)
        {
            if (!Physics2D.Raycast(transform.position, Vector3.up, 0.5f, ObstacleMask))
            {
                dir = Vector3.up * 1;
                _currentDirection = Direction.UP;
                StartMoving();
            }
            else
            {
                StopMoving();
            }
        }

        // --- MOVING DOWN --- //
        else if (_currentDirection == Direction.DOWN)
        {
            if (!Physics2D.Raycast(transform.position, Vector3.down, 0.5f, ObstacleMask))
            {
                dir = Vector3.up * -1;
                _currentDirection = Direction.DOWN;
                StartMoving();
            }
            else
            {
                StopMoving();
            }
        }

        if (!_isSliding)
        {
            _rb2D.velocity = Vector2.zero;
            _currInd = 0;
        }
        else
        {
            prevVel = _rb2D.velocity;
            _rb2D.velocity += (Vector2) dir.normalized * MoveSpeed * Time.deltaTime;
            // print(_rb2D.velocity);
            _rb2D.velocity = new Vector2(Mathf.Clamp(_rb2D.velocity.x, -25f, 25f), Mathf.Clamp(_rb2D.velocity.y, -25f, 25f));
           

            // GameManager_.Instance.MainCamera.GetComponent<CameraBehaviour>().Target2 = transform.position + dir.normalized * 5;
            //if (_currInd < _shadows.Count)
            //{
            //    if (_rb2D.velocity.magnitude > ((Vector2) dir.normalized * MoveSpeed).magnitude 
            //        / _shadows.Count * (_currInd + 1))
            //    {
            //        _shadows[_currInd].position = transform.position;
            //        _currInd++;
            //    }
            //}
            // print(_rb2D.velocity.magnitude);
        }
    }

    private void StartMoving()
    {
        // _initialPos = transform.position;
        _isSliding = true;
        // DashShadow.rotation = Quaternion.Euler(0, 0, -90 * (int)_currentDirection);
        // StartCoroutine(ShowShadow());
    }

    // IEnumerator ShowShadow()
    // {
        // foreach (Transform t in _shadows)
        // { 
           //  yield return new WaitForSeconds(0.1f);
            // t.gameObject.SetActive(true);
            
        // }
    // }

    private void StopMoving()
    {
        print("Moved");
        dir = Vector3.zero;
        _currentDirection = Direction.UNKNOWN;
        float x = Mathf.Round(transform.position.x * 2) / 2;
        float y = Mathf.Round(transform.position.y * 2) / 2;
        transform.position = new Vector2(x, y);
        StopAllCoroutines();
        if (GameManager_.Instance.EnableFlipping)
        StartCoroutine(SpawnTile(_initialPos, transform.position));
        
        
        if (Physics2D.OverlapCircle(transform.position, 0.2f, GoalLayer) && _heldKey >= RequiredKey)
        {
            //print("Win");
            holder.audio.PlayOneShot(holder.audioList[2]);
            GameManager_.Instance.LoadNextScene();
            // win the stage
        }
        _initialPos = transform.position;
        _isSliding = false;
        
        // foreach (Transform t in _shadows)
        // {
            // t.gameObject.SetActive(false);
        // }
        // print(prevVel.magnitude / 100);
        
        prevVel = Vector2.zero;
       //  GameManager_.Instance.MainCamera.GetComponent<CameraBehaviour>().Target2 = null;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Key"))
        {
            _heldKey++;
            Destroy(collision.gameObject);
        }
    }

    private IEnumerator SpawnTile(Vector2 initialPos, Vector2 currPos)
    {
        bool isGood = GameManager_.Instance.GoodSide;
        GameObject toSpawn = isGood ? GoodTile : BadTile;
        Transform parent = isGood ? GoodTileHolder : BadTileHolder;
        LayerMask useMask = isGood ? GoodMask : BadMask;
        string toplay = isGood ? "GoodTileExplode" : "BadTileExplode";
        Vector2 path = (Vector2)currPos - initialPos;
        print(path);
        if (path.magnitude >= 1)
        {
            GameManager_.Instance.UIPlayer.UpdateMove(-1);
            GameManager_.Instance.SoundPlayer.PlayClip("Bump", 0.6f);
            GameManager_.Instance.ParticlePlayer.PlayEffect("BumpWall", transform.position + dir.normalized);
            // GameManager_.Instance.MainCamera.GetComponent<CameraBehaviour>().TriggerCameraShake(0.1f, prevVel.magnitude / 500);
            // print("Called");
            if (path.y == 0)
            {
                for (int i = 0; i < Mathf.Abs(path.x); ++i)
                {
                    Vector2 pos = new Vector2(initialPos.x + Mathf.Sign(path.x) * i, initialPos.y);
                    if (Physics2D.OverlapCircle(pos, 0.2f, GoalLayer) || !Physics2D.OverlapCircle(pos, 0.2f, CastableLayer)) continue;
                    Collider2D hit = Physics2D.OverlapCircle(pos, 0.2f, useMask);
                    if (hit != null)
                    {
                        if (isGood) GameManager_.Instance.ParticlePlayer.PlayEffect(toplay, pos);
                        // GameManager_.Instance.
                        Destroy(hit.gameObject);
                    }
                    else
                    {
                        // print("Spawned");
                        Transform newObj = Instantiate(toSpawn, pos, Quaternion.identity).transform;
                        newObj.GetComponent<Animator>().Play("BlockForm");
                        newObj.SetParent(parent);
                    }
                    yield return new WaitForSeconds(0.01f);
                }
            }
            else if (path.x == 0)
            {
                for (int i = 0; i < Mathf.Abs(path.y); ++i)
                {
                    Vector2 pos = new Vector2(initialPos.x, initialPos.y + Mathf.Sign(path.y) * i);
                    if (Physics2D.OverlapCircle(pos, 0.2f, GoalLayer) || !Physics2D.OverlapCircle(pos, 0.2f, CastableLayer)) continue;
                    Collider2D hit = Physics2D.OverlapCircle(pos, 0.2f, useMask);
                    if (hit != null)
                    {
                        GameManager_.Instance.ParticlePlayer.PlayEffect(toplay, pos);
                        Destroy(hit.gameObject);
                    }
                    else
                    {

                        Transform newObj = Instantiate(toSpawn, pos, Quaternion.identity).transform;
                        newObj.GetComponent<Animator>().Play("BlockForm");
                        newObj.SetParent(parent);
                    }
                    yield return new WaitForSeconds(0.01f);
                }
            }
            
        }
        Collider2D hitCurr = Physics2D.OverlapCircle(transform.position, 0.2f, useMask);
        if (hitCurr != null) Destroy(hitCurr.gameObject);
    }

}
