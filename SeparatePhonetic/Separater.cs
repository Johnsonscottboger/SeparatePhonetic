using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SeparatePhonetic
{
    public class Separater
    {
        /// <summary>
        /// 声母表
        /// </summary>
        private static List<string> _initials = new List<string>()
        {
            "b","p","m","f","d","t","n","l","g","k","h","j","q","x","zh","ch","sh","r","z","c","s","y","w"
        };

        /// <summary>
        /// 韵母表
        /// </summary>
        private static List<string> _vowels = new List<string>()
        {
            "a","e","i","o","u","v",
            "ai","ei","ui","ao","ou","iu","ie","ue","er",
            "an","en","in","un","vn",
            "ang","eng","ing","ong"
        };


        /// <summary>
        /// 拼音字符串
        /// </summary>
        public string Phonetic { get; private set; }



        public Separater(string phonetic)
        {
            if(string.IsNullOrWhiteSpace(phonetic))
                throw new ArgumentNullException(nameof(phonetic));
            var notPhoneticRegex = new Regex(@"[^a-zA-Z '’]+");
            if(notPhoneticRegex.IsMatch(phonetic))
            {
                throw new ArgumentException("输入的参数不是有效的拼音字母");
            }


            this.Phonetic = phonetic;
        }

        /// <summary>
        /// 切分
        /// </summary>
        /// <returns></returns>
        public List<string> Split()
        {
            /**
             * 过程描述：
             * 1.从StartIndex开始，取第一个，并且同索引中长度最长的。
             * 2.从上个声母之后，如果不是韵母，退出，否则，取第一个韵母，同索引中取最长的。
             * 3.从上个韵母之后，如果仍是韵母，
             *   如果上一个声母之后的韵母个数只有一个，则追加当前韵母，同索引中取最长的。
             *   否则退出。
             * 4.从上个韵母之后，是声母，退出。
             */

            //声母 initials
            //韵母 vowels
            var phoneticList = new List<string>();
            var list = new List<Tuple<CharType,string>>();
            var builder = new StringBuilder();

            var index = 0;
            var dict = GetCharTypeDictionary();
            while(index < dict.Count)
            {
                list.Clear();
                builder.Clear();

                if(index < dict.Count && dict[index].Item1 == CharType.Initials)
                {
                    //获取声母
                    var initials = dict[index].Item2;
                    list.Add(dict[index]);
                    index += initials == null ? 0 : initials.Length;
                }

                if(index < dict.Count && dict[index].Item1 == CharType.Vowels)
                {
                    //获取第一个韵母
                    var vowels1 = dict[index].Item2;
                    list.Add(dict[index]);
                    index += vowels1 == null ? 0 : vowels1.Length;

                    if(index < dict.Count && dict[index].Item1 == CharType.Vowels)
                    {
                        //获取第二个韵母
                        var vowels2 = dict[index].Item2;
                        list.Add(dict[index]);
                        index += vowels2 == null ? 0 : vowels2.Length;
                    }
                }

                //如果都是韵母，拆开
                if(list.All(p=>p.Item1 == CharType.Vowels))
                {
                    foreach(var item in list)
                    {
                        phoneticList.Add(item.Item2);
                    }
                }
                else
                {
                    foreach(var item in list)
                    {
                        builder.Append(item.Item2);
                    }
                    phoneticList.Add(builder.ToString());
                }
            }

            return phoneticList;
        }

        
        /// <summary>
        /// 获取拼音字典
        /// </summary>
        /// <returns></returns>
        private Dictionary<int,Tuple<CharType,string>> GetCharTypeDictionary()
        {
            //是否为声母
            Func<string,bool> IsInitials = (s) =>
            {
                return _initials.Any(p => p == s);
            };

            //是否为韵母
            Func<string,bool> IsVowels = (s) =>
            {
                return _vowels.Any(p => p == s);
            };

            //获取以i开始，至结尾长度递增的子字符串
            Func<string,int,List<string>> GetSubString = (s,i) =>
            {
                var _builder = new StringBuilder();
                var _list = new List<string>();
                var _length = s.Length - i;
                foreach(var l in Enumerable.Range(1,_length))
                {
                    _list.Add(s.Substring(i,l));
                }
                return _list;
            };

            var dict = new Dictionary<int,Tuple<CharType,string>>();
            
            var splitArray = this.Phonetic.Split(new char[] { '\'','’' },StringSplitOptions.RemoveEmptyEntries);
            foreach(var s in splitArray)
            {
                var index = 0;
                var length = s.Length;
                var dictCount = dict.Count;
                for(; index < length; index++)
                {
                    var currentIndex = dictCount + index;
                    var allSubStrings = GetSubString(s,index);
                    foreach(var subString in allSubStrings)
                    {
                        if(IsInitials(subString))
                        {
                            if(dict.ContainsKey(currentIndex))
                            {
                                dict[currentIndex] = new Tuple<CharType,string>(CharType.Initials,subString);
                            }
                            else
                            {
                                dict.Add(currentIndex,new Tuple<CharType,string>(CharType.Initials,subString));
                            }
                        }
                        else if(IsVowels(subString))
                        {
                            if(dict.ContainsKey(currentIndex))
                            {
                                dict[currentIndex] = new Tuple<CharType,string>(CharType.Vowels,subString);
                            }
                            else
                            {
                                dict.Add(currentIndex,new Tuple<CharType,string>(CharType.Vowels,subString));
                            }
                        }
                    }
                }
            }
            return dict;
        }
    }

    /// <summary>
    /// 字符串的类型
    /// </summary>
    public enum CharType
    {
        /// <summary>
        /// 声母
        /// </summary>
        Initials,

        /// <summary>
        /// 韵母
        /// </summary>
        Vowels
    }
}
