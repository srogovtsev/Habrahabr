using System;
using System.Diagnostics;

namespace SMAS
{
    class ARRAY
    {
        private ARRAY nextLevel = null;
        int[] keyResult = null;
		int[] keyLeft = null;
        int[] keyRight = null;
        int[] keyTemp = null;
        int countResult = 0;
        int countLeft = 0;
        int countRight = 0;
        int len;

        public ARRAY(int len)
        {
            this.len = len;
        }

        int upd() //пошаговое отложенное слияние двух массивов в один
        {
            var ops = 0;
            if (keyResult != null)
            {
                if (countLeft < len)
                {
                    if (countRight < len)
                    {
                        if (keyLeft[countLeft] < keyRight[countRight]) //если текущий элемент 1-го подмассива меньше 2-го - вносим его
                        {
                            keyResult[countResult] = keyLeft[countLeft];
                            countResult++;
                            countLeft++;
                            ops++;
                        }
                        else //если текущий элемент 2-го подмассива меньше 1-го - вносим его в массив слияния
                        {
                            keyResult[countResult] = keyRight[countRight];
                            countResult++;
                            countRight++;
                            ops++;
                        }
                    }
                    else  //значит 2-й массив уже весь перемещён и необходимо добавить сл. элемент из первого подмассива
                    {
                        keyResult[countResult] = keyLeft[countLeft];
                        countResult++;
                        countLeft++;
                        ops++;
                    }
                }
                else
                {
                    if (countRight < len)   //значит 1-й массив уже весь перемещён - добавляем сл. элемент из второго подмассива
                    {
                        keyResult[countResult] = keyRight[countRight];
                        countResult++;
                        countRight++;
                        ops++;
                    }
                    //в противном случае все элементы обоих массивов уже присутствуют в результирующем - ничего не делаем
                }

                if (countResult == len * 2)   //Если после очередного шага слияние завершилось
                {
                    if (nextLevel == null) {
                        //Console.WriteLine("Allocating level...");
                        nextLevel = new ARRAY(len * 2);  //Создаём сл. уровень, если он ещё не существует
                    }
                    ops += nextLevel.set(keyResult, len * 2);
                    keyResult = null;
                    keyLeft = null;
                    keyRight = null;
                    keyTemp = null;
                    ops++;
                }
            }

            ops += nextLevel?.upd() ?? 0;
            return ops;
        }

        int set(int[] Key, int Len)
        {
            Debug.Assert(len == Len);
            var ops = upd();
            if (keyTemp == null)
            {
                keyTemp = Key;
                ops++;
            }
            else
            {
                keyLeft = Key;
                ops++;
                keyRight = keyTemp;
                ops++;
                keyTemp = null;
                countResult = 0;
                countLeft = 0;
                countRight = 0;
                keyResult = new int[len * 2];
            }
            ops += upd();
            return ops;
        }

        public int set(int Key)
        {
            SetCount++;
            return set(new [] {Key}, 1);
        }

        public int SetCount { get; private set; } = 0;

        public bool get(int Key)
        {
            int l, r, i;
            if (keyTemp != null)
            {
                l = 0; r = len;
                while (l != r - 1)
                {
                    i = (l + r) / 2;
                    if (keyTemp[i] < Key) l = i;
                    else if (keyTemp[i] > Key) r = i;
                    else return true;
                }
                if (keyTemp[l] == Key) return true;
            }
            if (keyLeft != null)
            {
                l = 0; r = len;
                while (l != r - 1)
                {
                    i = (l + r) / 2;
                    if (keyLeft[i] < Key) l = i;
                    else if (keyLeft[i] > Key) r = i;
                    else return true;
                }
                if (keyLeft[l] == Key) return true;
            }
            if (keyRight != null)
            {
                l = 0; r = len;
                while (l != r - 1)
                {
                    i = (l + r) / 2;
                    if (keyRight[i] < Key) l = i;
                    else if (keyRight[i] > Key) r = i;
                    else return true;
                }
                if (keyRight[l] == Key) return true;
            }

            return nextLevel?.get(Key) ?? false;
        }
    };
}
