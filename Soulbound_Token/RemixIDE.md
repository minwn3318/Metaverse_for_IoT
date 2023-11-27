1. 리믹스에서 제공하느 지갑에서 스마트 컨트랙 배포하고 트랜잭션 확인
   => 트랜잭션 로그 확인

* 지갑 : 0x5B38Da6a701c568545dCfcB03FcB875f56beddC4(리믹스 제공)
* 베포환경(네트워크) : remix vm (shanghai)
---- 사진 ----   
   
트랜잭션 로그 내용   
```
status	: 0x1 Transaction mined and execution succeed
transaction hash  : 0x9cee417d1b971bc7393ebfd7f55ca01267372548469b1856b3de46de09be30b0
block hash : 0xa50cf7a7b3d0edf2f55429005e45426a71a08ebeaddc3644291e718f19d67eb7
block number : 1
contract address : 0xd9145CCE52D386f254917e481eB44e9943F39138
from : 0x5B38Da6a701c568545dCfcB03FcB875f56beddC4
to : HelloWorld.(constructor)
gas : gas
execution cost : 102251
input : 0x60806040526040518060400160405280600c81.......
decoded input	: {}
decoded output	: - 
logs	: []
```
로그내용 분석   
```
status : 상태를 말한다. 해당 트랜잭션이 성공적으로 완료하였는지 알려준다
transaction hash : 트랜잭션의 고유아이디
block hash : 블록의 고유 아이디 >> 블록안에 트랜잭션들이 들어간다
blcok number : 몇번째 블록인지 알려줌
contract address : 
from : 트랜잭션 발생 부분(현재 배포한 지갑이 트랜잭션을 발생했다 함)
to : 트랜잭션 받은 부분...?(컨트랙을 만든거하고 무슨 차이가??)
gas :
execution cost :
input :
decoded input :
decoded output :
logs : 
```

2. 내 메타마스크 연결해서 해당 컨트랙트 배포하기
   * 배포내용 : helloWorld.sol
```
// SPDX-License-Identifier: MIT
pragma solidity >= 0.7.0 < 0.9.0;

contract HelloWorld {
    string private  saying = "Hello World!";

    // function GetHello() public view returns(string)
    // {
    //     return saying;
    // }
}
```
: 함수는 존재하지 않고 private 접근지정자인 스트링변수만 존재
   => 이더스캔 확인
* 내용
```
Transaction Hash : 0x27e87e46a319362425bfd79e76a968ea16c13320252b8cd05440faa0514d6a9d 
Method : 함수가 존재하지 않아 id로 표시됨 0x60806040
Block : 4774037
From : 0xe04fE4634d37f6842B4378dD2B0c713e484CD923 = 내 지갑
out 타입으로 나옴
To : contract cerdatial(컨트랙트, 계약서도 지갑처럼 주소가 존재)
Value : 0
Txn Fee : 0.00026716
gas price : 2.61285383 
```
--- 사진첨부  --- >> 
