using UnityEngine;
public static class SphereMesh {
  public static readonly Vector3[] Vertices = new Vector3[42] {
new Vector3(0f, 0f, -0.5f), 
new Vector3(0.08122778f, -0.2499976f, -0.4253272f), 
new Vector3(-0.2126613f, -0.1545057f, -0.4253271f), 
new Vector3(0.2628649f, 0f, -0.4253258f), 
new Vector3(0.08122778f, 0.2499976f, -0.4253272f), 
new Vector3(-0.2126613f, 0.1545057f, -0.4253271f), 
new Vector3(-0.1314344f, -0.4045058f, -0.2628688f), 
new Vector3(0.138194f, -0.4253246f, -0.2236099f), 
new Vector3(0f, -0.5f, 0f), 
new Vector3(0.2938928f, -0.4045084f, 0f), 
new Vector3(-0.3618037f, -0.2628627f, -0.2236098f), 
new Vector3(-0.4253239f, 0f, -0.262868f), 
new Vector3(-0.4755289f, -0.1545063f, 0f), 
new Vector3(-0.2938928f, -0.4045084f, 0f), 
new Vector3(-0.3618037f, 0.2628627f, -0.2236098f), 
new Vector3(0.3440947f, -0.2499985f, -0.2628681f), 
new Vector3(0.4472131f, 0f, -0.2236078f), 
new Vector3(0.4755289f, -0.1545063f, 0f), 
new Vector3(0.4755289f, 0.1545063f, 0f), 
new Vector3(0.3440947f, 0.2499985f, -0.2628681f), 
new Vector3(-0.1314344f, 0.4045058f, -0.2628688f), 
new Vector3(-0.2938928f, 0.4045084f, 0f), 
new Vector3(-0.4755289f, 0.1545063f, 0f), 
new Vector3(-0.4472131f, 0f, 0.2236078f), 
new Vector3(0.3618037f, -0.2628627f, 0.2236098f), 
new Vector3(0.2938928f, 0.4045084f, 0f), 
new Vector3(0.138194f, 0.4253246f, -0.2236099f), 
new Vector3(0f, 0.5f, 0f), 
new Vector3(0.3618037f, 0.2628627f, 0.2236098f), 
new Vector3(-0.138194f, 0.4253246f, 0.2236099f), 
new Vector3(0.1314344f, -0.4045058f, 0.2628688f), 
new Vector3(0.2126613f, -0.1545057f, 0.4253271f), 
new Vector3(-0.138194f, -0.4253246f, 0.2236099f), 
new Vector3(-0.3440947f, -0.2499985f, 0.2628681f), 
new Vector3(-0.08122778f, -0.2499976f, 0.4253272f), 
new Vector3(0.1314344f, 0.4045058f, 0.2628688f), 
new Vector3(-0.08122778f, 0.2499976f, 0.4253272f), 
new Vector3(-0.2628649f, 0f, 0.4253258f), 
new Vector3(-0.3440947f, 0.2499985f, 0.2628681f), 
new Vector3(0f, 0f, 0.5f), 
new Vector3(0.2126613f, 0.1545057f, 0.4253271f), 
new Vector3(0.4253239f, 0f, 0.262868f), 
};
  public static readonly int[] Triangles = new int[240] {
0, 
1, 
2, 
0, 
3, 
1, 
0, 
4, 
3, 
0, 
5, 
4, 
2, 
5, 
0, 
1, 
6, 
2, 
1, 
7, 
6, 
7, 
8, 
6, 
7, 
9, 
8, 
2, 
6, 
10, 
10, 
11, 
2, 
11, 
5, 
2, 
10, 
12, 
11, 
10, 
13, 
12, 
8, 
13, 
6, 
6, 
13, 
10, 
11, 
14, 
5, 
3, 
15, 
1, 
1, 
15, 
7, 
15, 
9, 
7, 
3, 
16, 
15, 
16, 
17, 
15, 
17, 
9, 
15, 
16, 
18, 
17, 
4, 
19, 
3, 
3, 
19, 
16, 
19, 
18, 
16, 
5, 
14, 
20, 
14, 
21, 
20, 
14, 
22, 
21, 
12, 
22, 
11, 
11, 
22, 
14, 
12, 
23, 
22, 
17, 
24, 
9, 
25, 
18, 
19, 
26, 
25, 
19, 
26, 
27, 
25, 
21, 
27, 
20, 
20, 
27, 
26, 
4, 
26, 
19, 
4, 
20, 
26, 
5, 
20, 
4, 
25, 
28, 
18, 
21, 
29, 
27, 
9, 
24, 
30, 
24, 
31, 
30, 
9, 
30, 
8, 
8, 
30, 
32, 
8, 
32, 
13, 
13, 
32, 
33, 
32, 
34, 
33, 
31, 
34, 
30, 
30, 
34, 
32, 
13, 
33, 
12, 
12, 
33, 
23, 
27, 
29, 
35, 
29, 
36, 
35, 
27, 
35, 
25, 
25, 
35, 
28, 
34, 
37, 
33, 
33, 
37, 
23, 
23, 
37, 
38, 
37, 
36, 
38, 
38, 
36, 
29, 
22, 
23, 
38, 
22, 
38, 
21, 
21, 
38, 
29, 
34, 
39, 
37, 
37, 
39, 
36, 
31, 
39, 
34, 
40, 
39, 
31, 
36, 
39, 
40, 
36, 
40, 
35, 
40, 
31, 
41, 
28, 
40, 
41, 
35, 
40, 
28, 
18, 
28, 
41, 
18, 
41, 
17, 
17, 
41, 
24, 
41, 
31, 
24, 
};
}