# IoT-216 강의: Network-Programming / C# winForm
------------
### [과제 및 시행착오]
* Lect 2) 소켓 생성 및 활용, 스레드 생성 및 활용
  * FormServer 프로젝트의 스레드 내 문제 발생 --> 에러는 FormClient측(socket.connect)에서 떠서 찾는 데 오래걸림
  * 스레드내에서 폼의 컨트롤 속성(tbServer.Text) 직접 변경 시, 에러 
    * invoke, delgate(내일 배울 것) 
    * 위 대신에 글로벌변수에 저장 후 타이머tick마다 tbServer.Text에 
* Lect 3) 프로젝트 2개(서버, 클라이언트)생성해서 1패킷 주고 받기
- ComServer프로젝트의 FormServer.cs
  * listener.AcceptTcpClient()
    * blocking - 연결 받기까지 다른 작업(창 이동 등) 불가, 스레드 abort, suspend 등을 해도 진행중인 스레드 모두 끝내고 적용되므로 문제
    * → if(listener.Pending())내에 작성	 // 보류 중인 연결 있을 때에만 accept하도록 해야 함
  * listener: 로컬포트에 일정한 포트 넘버를 계속 감시하는 기능 수행
    * start후 계속 살아있는 상태로 stop하지 않는게 좋다 (스레드와 운명 같이하도록)- 경험적
  * 스레드 무한 루프 끝에 Thread.Sleep(100);  //약간의(10~100정도)딜레이를 주는게 좋다
    * cpu점유 등과 관련 → 부하가 커서? 다른 처리속도 느려짐
  * Timer: 타이머 정확x →  사용시 검증 중요, 안쓸수 있다면 안쓰는게 좋다

  * 연결 accept - TcpClient 대신 socket 사용해보기
    * listener.AcceptSocket()
    * socket.Available, Receive(), RemoteEndPoint()
    * EndPoint: 통신하는 양 끝점
    * IPEndPoint
    
- ComClient프로젝트의 FormClient.cs
* 설정 파일 - 사용자 환경 저장(설치작업, customizing) - ini파일 , 레지스트리
  * ini파일: initialize 의미
    * 폼 load될 때, ini에서 초기 값 가져와 설정 / 폼closing 시, 최종값으로 ini 갱신
    * kernel32.dll파일 import 후 사용할 메서드 선언(GetPrivateProfileString / write~)
    * > +) 폼위치 : Location = new Point(x,y)--> x,y각각 넣을수없음, 폼 사이즈도
* Lect 4) 서버와 클라이언트 간 여러 데이터 주고 받기

-------------------
### [수업 이론 내용]
* 통신 장비
  * 허브
  * 라우터 
    * L2 :DHCP(dynamic host configuration protocol-호스트의 IP주소와 각종 TCP/IP 프로토콜의 기본 설정을 클라이언트에게 자동 제공하는 프로토콜), 고속스위칭
    * L3 :방화벽, 보안(물리적)
    * L4 :로드밸런싱
* cmd명령어
  * ipconfig: 내 컴퓨터의 네트워크 환경(IP정보) 확인 가능
  * ping(Packet INternet Groper): 대상 컴을 향해 일정 크기의 패킷을 보내고 이에 대한 응답을 받아 대상 컴퓨터의 동작 여부나 네트워크 상태 파악 가능
     > ping [IP 또는 도메인]
  * nsLookup: DNS에 질의하여, 도메인의 정보를 조회, 어떤 주소의 실제 IP주소알 수 o
  * netstat: 현재 내 컴에서 연결중이거나 연결대기중인 상태의 port 확인 가능
  * route: 현재 넷상의 모든 연결
* loopback: 컴퓨터환경에서 자기 자신에 접근하는 경우
  * 127.0.0.1
    * OS자체적으로 제공하며, 항상 고정된 자기자신을 가리키는 IP, 예약된 ip주소
    * os에서 가상 지원, 랜카드 등 디바이스 자체를 통과하지 않고 sw적으로 처리됨
  * localhost
    * 호스트네임, 자신의 ip로 직접 접근 시, localhost로 접근하는게 더 빠르고 자원 덜 씀
* 포트번호
    * 80번: internet과 통신 시 사용
    * 23번: MS-SQL
* 프로토콜: 컴들이 네트워크를 통해 데이터를 주고받기 위한 통신규약
    * 네트워크에서 데이터를 주고 받기 위해서는 그 네트워크에서 사용되는 프로토콜 따라야
* TCP(transmission control protocol): 연결형 서비스 지원하는 전송계층 프로토콜, 신뢰성o, unicast
    * IP와 포트 필요: IP는 호스트까지 연결하지만(계층차이), tcp는 호스트 내 포트까지 연결하며 해당 포트에서 기다리고 응용 프로그램까지 도달
* TcpListener/ TcpClient: .NET프레임워크가 TCP/IP통신을 위해 제공하는 클래스
    * 내부적으로 System.Net.Sockets.Socket클래스 기능들 사용
  * Tcpclient클래스 - 클라이언트
    * TcpClient.Client(내부소켓).RemoteEndPoint(원격 끝점)
  * TcpListener클래스 -서버
    * AcceptTcpClient: 포트를 열고 메서드를 통해 클라이언트 접속을 대기하고 있다가 접속 요청이 오면 이를 받아들여 tcpClient객체를 생성하여 리턴
    * > blocking 주의: 연결전까지 대기만 가능, 스레드 상태 변화 불가(진행중인 스레드 마치고 상태변화 적용 되므로)
    * AcceptSocket: AcceptTcpClient()대신 사용 시 tcpclient 객체 대신 low level의 Socket객체 사용 가능
    * Pending: 보류 중인 연결 존재 시
 
* socket: 네트워크 상에서 동작하는 프로그램 간 통신의 종착점(Endpoint)
    * 응용프로그램에서 tcp/ip를 이용하는 창구역할, 통신을 위한 통로
    * 프로그램이 네트워크에서 데이터를 통신할 수 있도록 연결해주는 연결부
  * TCP/IP기반의 Stream방식
  * UDP/IP기반의 Datagram방식
* Socket클래스
    * tcp관련 클래스는 TCP/IP만 지원, 소켓클래스는 IP외에도 다양한 네트워크
    * Available: 읽을 수 있는 데이터의 양
    * Receive: 바인딩 된 소켓 정보 받기, 받은 바이트 수 반환
    * RemoteEndPoint: 원격 끝점 -->EndPoint로, cast연산 통해 IPEndPoint(O)

==> 소켓 연결 대기 중에는 Form다른 동작x --> 스레드 필요
* 스레드(thread): 하나의 프로그램에서 한번에, 동시에 많은 일 처리 가능
  * 상태: suspend, abort 등
   * 교차 스레드(cross Thread): 컴포넌트 생성한 스레드와 호출한 스레드가 다를 때 발생
    > 해당 스레드를 만든 곳이 아니면 동작x
  * 해결: delegate: 대리자로서 메서드를 다른 메서드의 인수로 전달

* Delegate(대리자): 대신 수행, 메서드에 대한 참조를 가리키는 형식으로 메서드 간접 호출 가능
    * 함수의 포인터라고 생각하면 편함
    * delegate선언, 그곳에 맞는 콜백함수를 선언하여 처리
    * 프로젝트 내용 중 addText함수: 컨트롤.InvokeRequired발생 시, invoke로 실행
* Timer 도구
    * Tick 이벤트: 타이머의 일정 시간 경과 시 마다 --> 부정확, 안 쓸수 있다면 안쓰는게 좋다
---------
