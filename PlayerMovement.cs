using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerMovement : MonoBehaviour
{
    //apply forces to the rigid body in order for the player to move ist�llet
    
    // definera variabler
    public healthBar healthBar;
    public textScripts textScripts;
    [SerializeField] private float playerSpeed; //SF s� dene accessible fr�n unity
    private Rigidbody2D body;
    private float scaleDownSize = 0.6f; 
    private Animator animator;
    private bool grounded;
    private bool insideShop = false;
    private float lockPos = 0;
    private float Stamina = 0;
    private bool ifSprinting = false;
    private int coins = 0;
    private float tid;
    private float maxHealth = 100;
    private float currentHealth;

    void Start()
    { 
        body = GetComponent<Rigidbody2D>(); // checkar om det finns en component "Rigidbody2D", sen store den i body variabeln. "used to get references"
        animator = GetComponent<Animator>(); // grab reference for animator from object
        currentHealth = maxHealth; //  set health t max i b�rjan n�r spelet �ppnas
    }

    // Update is called once per frame
    void Update()
    {
        // uppdatera health, stamina och tiden
        healthBar.setHealth(currentHealth);
        textScripts.updateStamina(Stamina);
        textScripts.updateTimer(tid);

        //inputGetAxis horizontal A ger v�rde som n�rmar sig -1 och D v�rde som n�rmar sig +1, anv�nds ist f�r if else.
        float horizontalInput = Input.GetAxis("Horizontal"); 


        tid += Time.deltaTime;
        
        if (currentHealth < 100)
        {
            currentHealth += (Time.deltaTime * 2); // uppdateras med tiden s� de inte e dependent p� datorns performance
        } 
        if (currentHealth <= 0)
        {
            GameOver(); // inte fixat game over funktionen �n de va bara f�r att testa
        }

        if (tid >= 4)
        {
            tid = 0;
            FindObjectOfType<SpawnerScript>().spawnCoin(-5, 8, -2); // spawna varje 4 sekunder
            FindObjectOfType<SpawnerScript>().spawnMob(10, 15, -17);

        }

        if (Stamina <= 500)
        {
            Stamina += (31 * Time.deltaTime);

        }

        transform.rotation = Quaternion.Euler(lockPos, lockPos, lockPos); // s� den inte har n�gon rotation (2d spel)

        //vector2 f�r de 2D
        if (grounded)
        {
            body.velocity = new Vector2(horizontalInput * playerSpeed, body.velocity.y); // x,y,x
        }


        // om x scale f�r gubben �r positiv = h�ger, om scale e negativ kollar den v�nster.
        if (horizontalInput > 0f)
        {
            transform.localScale = Vector2.one * scaleDownSize;
        }
        if (horizontalInput < 0f)
        {
            transform.localScale = new Vector2((-1 * scaleDownSize), (1 * scaleDownSize));
        }


        // movement

        if (Input.GetKey(KeyCode.LeftShift) && horizontalInput > 0 && grounded && Stamina >= 10) //getkey returns true or false. kan bara hoppa om mane grounded.
        {
            Sprint(2);
        }
        if (Input.GetKey(KeyCode.LeftShift) && horizontalInput < 0 && grounded && Stamina >= 10) //getkey returns true or false. kan bara hoppa om mane grounded.
        {
            Sprint(-2);
        }
        if (Input.GetKey(KeyCode.Space) && horizontalInput >= 0 && grounded && Stamina >= 100) //getkey returns true or false. kan bara hoppa om mane grounded.
        {
            Jump(2);
        }
        if (Input.GetKey(KeyCode.Space) && horizontalInput <= 0 && grounded && Stamina >= 100) //getkey returns true or false. kan bara hoppa om mane grounded.
        {
            Jump(-2);
        }

        if (insideShop == true) // inte fixat shoppen �n
        {
            if (Input.GetKey(KeyCode.Q))
            {
                textScripts.hideShop();
                textScripts.enterShopText();
                insideShop = false;
                TP(0, 0);

            }
        }

        // set animator parameters (g�r den antingen true eller false s� den visar idle eller run).
        animator.SetBool("Run", horizontalInput != 0); // om keys not pressed horizInput kmr va 0 (d� den ska va idle), om pressed antingen de kmr va -1 eller 1, allts� != 0.
        animator.SetBool("Grounded", grounded);

        ifSprinting = false;

    }

    private void Jump(int x) // funktionen f�r att hoppa
    {
        if (ifSprinting) // hoppa i samma fart som man springer, borde finnas finare l�sning t detta men lallish
        {
            body.velocity = new Vector2((playerSpeed * x), (playerSpeed * 1.8f));
        } // �ndrar bara y (jump)
        else
        {
            body.velocity = new Vector2((body.velocity.x), (playerSpeed * 1.8f));
        }
        grounded = false;
        Stamina -= 100;
    }

    private void Sprint(int x)
    {
        ifSprinting = true;
        body.velocity = new Vector2((x * playerSpeed), body.velocity.y);
        Stamina -= 70 * Time.deltaTime;
    }


    private void TP(int x, int y) // tp till givna kordinater
    {
        transform.position = new Vector2(x, y);
    }

    private void takeDamage(int damage)
    {
        currentHealth -= damage;
        healthBar.setHealth(currentHealth);

    }
    public void addCoin()
    {
        coins += 1;
    }
    private void GameOver() // inte klar
    {
        FindObjectOfType<CSV>().importCoins(coins);
        FindObjectOfType<CSV>().writeCSV();
        textScripts.showGameover();
    }


    //to know when player colldes with object. 
    private void OnCollisionEnter2D(Collision2D collision)
    {

        if (collision.gameObject.tag == "Ground")
        {
            print("grounded");
            grounded = true;
        }

        // denna funka bara under collision inte triggerenter
        if (collision.gameObject.tag == "mob1") 
        {
            print("collided with mob");
            takeDamage(40);
        }

    }

    // samma, know n�r den har "trigger" med ett objekt
    private void OnTriggerEnter2D(Collider2D collision)
    {

        if (collision.gameObject.tag == "coin")
        {
            coins += 1;
            textScripts.coinNumberRN(coins);
            Destroy(collision.gameObject);
        }

        if (collision.gameObject.tag == "testTag")
        {
            Stamina = 0;
            print("Stamina Reset!");
        }
        if (collision.gameObject.tag == "addStamina")
        {
            if (Stamina < 400)
            {
                Stamina += 100;
                print("Stamina Added!");
            }
        }

        if (collision.gameObject.name == "level1entry")
        {
            TP(0, -18);
        }
    }

    private void OnTriggerStay2D(Collider2D collision) // medans gubben frf collidar med ett object, ass� st�r p� de tex
    {
        if (collision.gameObject.name == "shopBuilding")
        {
            textScripts.showShop();
            if (Input.GetKey(KeyCode.E))
            {
                insideShop = true;
                grounded = false;
                TP(-15, 26);
                textScripts.insideShopText();
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision) // n�r gubben slutar collida, s� efter man g�tt bort fr�n objektet
    {

        if (collision.gameObject.name == "shopBuilding")
        {
            if (insideShop == false)
            {
                textScripts.hideShop();
            }
        }
    }

    void OnMouseDown() // TESSSSSSTT
    {
        if (gameObject) // vrf funkar de inte f�r gameObject.name och .tag?!
        {
            coins -= 1;
            textScripts.coinNumberRN(coins);
        }
        if (gameObject.tag == "coin")
        {
            Destroy(gameObject);
        }
    }
}
