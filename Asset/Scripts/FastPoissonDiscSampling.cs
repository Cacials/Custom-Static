using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FastPoissonDiscSampling : MonoBehaviour
{

    //随机点，在与中心点[r,2r)内找点，定义常量k,在每个点附近内随机找k次，找不到则认为附近已经没有Valid点
    //划分空间，平面分成mxn，每个格子保存内部的点，判断valid时只需要和附近格子做判断
    //尽量每个格子只有一个点，保证对角线长度>=r，则任意两点<r，最多内部只有一个点
    public static List<Vector2> GenerateScatter(int seed, float width, float height, float r, int k = 30)
    {
        var n = 2;
        //格子大小
        var cellSize = r / Mathf.Sqrt(n);
        //行列数
        int cols = Mathf.CeilToInt(width / cellSize);
        int rows = Mathf.CeilToInt(height / cellSize);
        
        // cells记录了所有合法的点
        List<Vector2> points = new List<Vector2>();
        
        // 每个cell内的点在cells里的索引，-1表示没有点
        int[,] grids = new int[rows, cols];
        for (var i = 0; i < rows; ++i) 
        {
            for (var j = 0; j < cols; ++j) 
            {
                grids[i, j] = -1;
            }
        }
        
        // STEP 1
        Random.InitState(seed);
        
        // 随机选一个起始点
        var x0 = new Vector2(Random.Range(0,width), Random.Range(0,height));
        int col = (int)Mathf.Floor(x0.x / cellSize);
        int row = (int)Mathf.Floor(x0.y / cellSize);
        
        //起始点index
        var x0_idx = points.Count;
        points.Add(x0);
        grids[row, col] = x0_idx;

        //附近还有拥有合法点 的点的List
        var active_list = new List<int>();
        active_list.Add(x0_idx);

        //直到没有更多的合法点
        while (active_list.Count > 0)
        {

            //一个随机待处理点
            var xi_idx = active_list[Random.Range(0, active_list.Count)];
            var xi = points[xi_idx];
            var found = false;

            //k次内在xi距离内的点，合法性判断，找不到就从actice_list中去掉，附近已经没用了

            for (int i = 0; i < k; i++)
            {
                Vector2 dir = Random.insideUnitCircle;
                Vector2 xk = xi + (dir.normalized * r + dir * r); // [r,2r)
                if (xk.x < 0 || xk.x >= width || xk.y < 0 || xk.y >= height)
                {
                    continue;
                }

                col = (int)Mathf.Floor(xk.x / cellSize);
                row = (int)Mathf.Floor(xk.y / cellSize);

                //已经记录过的含点的格子
                if (grids[row, col] != -1)
                {
                    continue;
                }

                // 要判断xk的合法性，就是要判断附近有没有点与xk的距离小于r
                // 由于cell的边长小于r，所以只测试xk所在的cell的九宫格是不够的（考虑xk正好处于cell的边缘的情况）
                // 正确做法是以xk为中心，做一个边长为2r的正方形，测试这个正方形覆盖到的所有cell
                var ok = true;
                var min_r = (int)Mathf.Floor((xk.y - r) / cellSize);
                var max_r = (int)Mathf.Floor((xk.y + r) / cellSize);
                var min_c = (int)Mathf.Floor((xk.x - r) / cellSize);
                var max_c = (int)Mathf.Floor((xk.x + r) / cellSize);
                for (var or = min_r; or <= max_r; ++or)
                {
                    if (or < 0 || or >= rows)
                    {
                        continue;
                    }

                    for (var oc = min_c; oc <= max_c; ++oc)
                    {
                        if (oc < 0 || oc >= cols)
                        {
                            continue;
                        }

                        var xj_idx = grids[or, oc];
                        if (xj_idx != -1)
                        {
                            var xj = points[xj_idx];
                            var dist = (xj - xk).magnitude;
                            if (dist < r)
                            {
                                ok = false;
                                goto end_of_distance_check;
                            }
                        }
                    }
                }

                end_of_distance_check:
                if (ok)
                {
                    var xk_idx = points.Count;
                    points.Add(xk);

                    grids[row, col] = xk_idx;
                    active_list.Add(xk_idx);

                    found = true;
                    break;

                }
            }
            
            if (!found)
            {
                active_list.Remove(xi_idx);
            }
        }


        return points;
    }
    
    private bool isValid()
    {
        return false;
    }
}
