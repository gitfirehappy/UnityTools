using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;

public class DialogManager : MonoBehaviour
{
    /// <summary>
    /// 对话文本文件，csv格式
    /// </summary>
    public TextAsset dialogDataFile;

    /// <summary>
    /// 左侧角色图像
    /// </summary>
    public SpriteRenderer spriteLeft;
    
    /// <summary>
    /// 右侧角色图像
    /// </summary>
    public SpriteRenderer spriteRight;

    /// <summary>
    /// 角色名称文本
    /// </summary>
    public TMP_Text nameText;
    
    /// <summary>
    /// 对话内容文本
    /// </summary>
    public TMP_Text dialogText;
    
    /// <summary>
    /// 角色图片列表
    /// </summary>
    public List<Sprite> sprites = new List<Sprite>();
    
    /// <summary>
    /// 角色名称对应图片字典
    /// </summary>
    public Dictionary<string,Sprite> imageDict = new Dictionary<string, Sprite>();

    /// <summary>
    /// 当前对话索引
    /// </summary>
    public int dialogIndex = 0;

    /// <summary>
    /// 对话文本按行分割
    /// </summary>
    public string[] dialogRows;

    /// <summary>
    /// 对话继续按钮
    /// </summary>
    public Button nextButton;
    
    /// <summary>
    /// 选项按钮预制体
    /// </summary>
    public GameObject optionButton;
    
    /// <summary>
    /// 选项按钮父节点，用于自动排列
    /// </summary>
    public Transform buttonGroup;
    
    public List<Person> persons = new List<Person>();
    
    private void Start()
    {
        ReadText(dialogDataFile);
        ShowDialogRow();
    }

    private void Awake()
    {
        imageDict["小红"] = sprites[0];
        imageDict["小明"] = sprites[1];
        Person person1 = new Person();
        person1.name = "小红";
        persons.Add(person1);
        Person person2 = new Person();
        person2.name = "小明";
        persons.Add(person2);
    }

    public void UpdateText(string _name, string _text)
    {
        nameText.text = _name;
        dialogText.text = _text;
    }

    public void UpdateImage(string _name,string _position)
    {
        if (_position == "左")
        {
            spriteLeft.sprite = imageDict[_name];
        }
        else if (_position == "右")
        {
            spriteRight.sprite = imageDict[_name];
        }
    }

    public void ReadText(TextAsset _textAsset)
    {
        dialogRows = _textAsset.text.Split('\n');
        // foreach (var row in rows)
        // {
        //     string[] cell = row.Split(',');
        // }
        Debug.Log("读取文件成功");
    }

    public void ShowDialogRow()
    {
        for (int i = 0; i < dialogRows.Length; i++)
        {
            string[] cells = dialogRows[i].Split(',');
            if (cells[0] == "#" && int.Parse(cells[1]) == dialogIndex)
            {
                UpdateText(cells[2], cells[4]);
                UpdateImage(cells[2], cells[3]);

                dialogIndex = int.Parse(cells[5]);
                nextButton.gameObject.SetActive(true);
                break;
            }
            else if (cells[0] == "&" && int.Parse(cells[1]) == dialogIndex)
            {
                nextButton.gameObject.SetActive(false);
                GenerateOptions(i);
            }
            else if (cells[0] == "END" && int.Parse(cells[1]) == dialogIndex)
            {
                Debug.Log("对话结束");
            }
        }
    }

    public void OnClickNext()
    {
        ShowDialogRow();
    }

    public void GenerateOptions(int _index)
    {
        string[] cells =  dialogRows[_index].Split(',');
        if (cells[0] == "&")
        {
            GameObject button = Instantiate(optionButton, buttonGroup);
            //  绑定按钮事件
            button.GetComponentInChildren<TMP_Text>().text = cells[4];
            button.GetComponentInChildren< Button>().onClick.AddListener(delegate
            {
                OnOptoinClick(int.Parse(cells[5]));
                if (cells[6] != "")
                {
                    Debug.Log("添加按钮附加效果");
                    string[]  effect = cells[6].Split('@');
                    cells[7] = Regex.Replace(cells[7], @"[\r\n]", "");
                    OptionEffect(effect[0],int.Parse(effect[1]),cells[7]);
                }
                OnOptoinClick(int.Parse(cells[5]));
            });
            GenerateOptions(_index + 1);
        }
    }

    public void OnOptoinClick(int _id)
    {
        dialogIndex = _id;
        ShowDialogRow();
        for (int i = 0; i < buttonGroup.childCount; i++)
        {
            Destroy(buttonGroup.GetChild(i).gameObject);
        }
    }

    public void OptionEffect(string _effect,int _param,string _target)
    {
        if (_effect == "好感度加")
        {
            foreach (var person in persons)
            {
                if (person.name == _target)
                {
                    person.likeValue += _param;
                    Debug.Log("好感度增加：" + person.name + " " + person.likeValue);
                }
            }
        }
        else if (_effect == "体力值加")
        {
            foreach (var person in persons)
            {
                if (person.name == _target)
                {
                    person.strengthValue += _param;
                    Debug.Log("体力值增加：" + person.name + " " + person.strengthValue);
                }
            }
        }
    }
}
