# AccessDemo
C#影像指標存取各種方式效率測試
我的測試結果


x64 64bit mode
Version: 4.0.30319.42000


[測試資料搬移速度]

Copy by bytes
cost : 4701
fps : 106

Copy by uint
cost : 1873
fps : 266

Copy by ulong
cost : 1442
fps : 346

[測試RGB各項存取處理搬移速度]

deal by bytes
cost : 4655
fps : 107

deal by uint way 1 (combine by bitwise)
cost : 4222
fps : 118

deal by uint way 2 (combine by byte loc)
cost : 3219
fps : 155

deal by ulong way 1 (combine by bitwise)
cost : 2758
fps : 181

deal by ulong way 2 (combine by byte loc)
cost : 2302
fps : 217
