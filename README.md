# IoT-216 강의: Network-Programming / C# winForm
---------
### [실습 및 시행착오]
---------
- Lect 1 - 이론
- Lect 2 - 소켓 생성 및 활용, 스레드 생성 및 활용
  - FormServer 프로젝트의 스레드 내 문제 발생 --> 에러는 FormClient측(socket.connect)에서 떠서 찾는 데 오래걸림
  - 스레드내에서 폼의 컨트롤 속성(tbServer.Text) 직접 변경 시, 에러 
    - invoke, delgate(내일 배울 것) 
    - 위 대신에 글로벌변수에 저장 후 타이머tick마다 tbServer.Text에 
- Lect 3 - 프로젝트 2개(서버, 클라이언트)생성해서 1패킷 주고 받기 / 설치파일(ini)
  - listener.AcceptTcpClient()
    * blocking - 연결 받기까지 다른 작업(창 이동 등) 불가, 스레드 abort, suspend 등을 해도 진행중인 스레드 모두 끝내고 적용되므로 문제
    * → if(listener.Pending())내에 작성	 // 보류 중인 연결 있을 때에만 accept하도록 해야 함
  - listener: 로컬포트에 일정한 포트 넘버를 계속 감시하는 기능 수행
    * start후 계속 살아있는 상태로 stop하지 않는게 좋다 (스레드와 운명 같이하도록)- 경험적
  * 스레드 무한 루프 끝에 Thread.Sleep(100);  //약간의(10~100정도)딜레이를 주는게 좋다
    * cpu점유 등과 관련 → 부하가 커서? 다른 처리속도 느려짐
  * Timer: 타이머 정확x →  사용시 검증 중요, 안쓸수 있다면 안쓰는게 좋다
  * 연결 accept - TcpClient 대신 socket 사용해보기
    * listener.AcceptSocket()
    * socket.Available, Receive(), RemoteEndPoint()
    * EndPoint: 통신하는 양 끝점
    * IPEndPoint
  * 설정 파일 - 사용자 환경 저장(설치작업, customizing) - ini파일 , 레지스트리 ==> ini파일: initialize 의미
    * 폼 load될 때, ini에서 초기 값 가져와 설정 / 폼closing 시, 최종값으로 ini 갱신
    * kernel32.dll파일 import 후 사용할 메서드 선언(GetPrivateProfileString / write~)
    * > +) 폼위치 : Location = new Point(x,y)--> x,y각각 넣을수없음, 폼 사이즈도
- Lect 4
  - 컨트롤 클래스 라이브러리에 설정파일(ini)조작하는 함수 담기
    - 새 프로젝트(컨트롤 클래스 라이브러리)로 작성 후 빌드 → 사용할 프로젝트의 참조로 추가(.dll)
    - 라이브러리에서 작성한 다른 클래스 mylib를 프로젝트에서 쓸 때, using, new필요 → 번거로움 
    - → static 붙이기 ⇒ class앞에 static, 그 안의 모든 함수들도 static이어야 ⇒ 클래스명.함수명() 형태로 사용 가능
  - 서버 측에서도 연결 끊지 않고 메세지 전송하기
    - ServerProcess()함수 마지막에 클라이언트에서처럼 Receive
- Lect 6: Socket Programming(low level) 
  - 다양한 방식으로 Send하기
    - 서버에서 텍스트박스(tbSend)에 문자열 입력 후 엔터 --> 클라이언트의 텍스트박스(tbReceive)에 입력
    - 텍스트 박스.SelectedText만 보내기
     - ContextMenuScript로 ==> tbSend에서 contextmenuscript로 연결해야
  - TcpListener, TcpClient 클래스 사용한 코드 --> 모두 Socket 사용해서 바꾸기
    - Bind(), Listen(), Accept()
  - IPAddress ip = new IPAddress(object[]) --> {127,0,0,1}
- Lect 7: 1대 1 채팅 프로그램       ==> FrmChat.cs , FrmConnectSetting.cs
  - thread.IsBackground=true;  //주 스레드 와 같이 종료
  - 기능
    - form load: Server상태, 연결 대기
    - 메뉴바 communication의 NewConnect: 연결할 ip와 port 입력하여 그 주소로 연결 및 통신
    - --> ip, port입력 폼 생성: 이전에 연결됐던 곳의 값 입력되어있음
    - --> 해당 값 변경 시 유효한 ip, port인지 검사 후 connect
    - 메뉴바 communication의 연결대기: Server로서 대기 상태로 전환
  - 시행착오
    - 어제의 의문사항: 연결없이 send버튼 클릭 시 에러메세지 안뜸 -->런타임시 걸리는 조건이라서
    - --> try catch 이전에 if문으로 처리
- Lect 8: 채팅 프로그램 업글
  - 텍스트박스 하나로 합치고 프로그램표시글, 수신된 내용, 송신한 내용으로 나눠서 표시하기
  - 연결 끊기 메뉴 추가
  - 현재의 연결 상태에 따라 statusStrip에 색깔로 표현하기
    - SessionProcess / threadSession: 현재 연결 체크(sock.Connected) 후 아래 함수 호출 
    - cbSessionState / SessionState
    - → 없을 시 크로스스레드 발생 
    - → 해당 stripstatusLabel에 대해서가 x, statusstrip전체 에 대한 nvokeRequired
==> 현재 남은 문제; 상대측 연결이 끊기고 나서 내가 Send를 두번 해야만 연결끊어짐을 인식함
- Lect 9:  문제 해결 및 1:N통신으로 업그레이드
  - 연결 관리 문제: socket의 null여부와 sock.Connected만으로는 연결 끊기는 것 인식하기 어려웠음
  - ==> isAlive 함수 만들기
    - sock.Poll(1000, SelectMode.Read); // 1000마이크로초(1ms초) 동안 응답대기, 해당 소켓이 readable인지
    - bool r2 = ss.Available == 0; // 읽을 수 있는 데이터가 없으면 true
    - try안에 ss.Send(new byte[1], 0, SocketFlags.OutOfBand); //오류는 소켓 연결과 관련된 것-->catch에서 false를 넘겨줌
  - 여러 클라이언트의 연결 받기
    - 소켓 리스트 생성해 저장
    - 현재 상태가 Server인지 Client인지 구분이 필요해짐 --> Mode 라는 int변수로 지정해줌
    - ReadProcess에서는 Server의 경우 소켓 리스트를 돌면서 기존의 ReadProcess 수행 필요
- Lect 10: 1:N 통신 - 서버측, 클라이언트 중 하나 선택해서 Send, SessiongPrcoess 수행하기
  - statusbar의 요소 중 dropdown으로, 연결된 클라이언트들의 주소 목록 만들기
    - 해당 소켓 accept후 remoteEndPoint의 주소내용을 dropdown item에 추가
  - 해당 아이템 클릭 시, 그 주소의 클라이언트와의 연결정보 표시 및 Send되도록
    - foreach(Socket ss in socks) 클릭된 text와 같은 주소 정보인 소켓 찾기
    - 위의 식 람다식으로 한 줄에 구현 가능( FindIndex함수)
    - >sock =socks[socks.FindIndex(s=> s.RemoteEndPoint.toString()==e.ClickedItem.Text)];
  - send나 session process 등 기존 함수들은 1대 1 통신으로 소켓이 하나, 이를 글로벌 변수인 sock으로 지정해 사용했음  1대 다의 경우 여러 소켓이 존재하게 되었고, 함수들을 바꿀 필요가 있어짐   → 변경 최소화 위해 기존의 sock에는 dropdown 에서 선택된 소켓으로 지정함   
- Lect 11: 에뮬레이터 구성 - 가상 장비 설계 / 클라이언트 모드
  - 에뮬레이터: 단말기를 흉내내는 프로그램 --> PC통신
  - 윈폼 도구
    - textbox - maxLength속성: 고정길이까지만 입력 가능
    - DateTimePicker - 달력 형태로 날짜를 선택할 수 있음
  - Timer-Tick 이벤트 - Send Package
    - 모든 텍스트박스 내용 종합해서 보냄
    - 타이머 중복 수행 방지 필요!! - tick이벤트 수행 도중 다시 타이머가 돌아오면서 중복수행될 수 있다.
    - > 시작 시, timer stop시키고 수행한 후, 마지막에 다시 start해야함

-------------------
### [수업 이론 내용] 
--------------------
1. 네트워크 구조
* 프로토콜: 인터넷 상에서 컴 간 데이터 주고 받기 위한 약속된 형식 → 통신 규약
* osi 7 계층: 컴퓨터 네트워크 프로토콜 형식과 통신을 계층으로 구분하여 규정→ 프로토콜을 기능별 구분한 것 → 프로그래머는 os에서 제공하는 서비스 사용; 전송~응용계층 까지
* 전송계층
  * TCP(transmission control protocol): 연결형 서비스 지원하는 전송계층 프로토콜, 신뢰성o, unicast
    * IP와 포트 필요: IP는 호스트까지 연결하지만(계층차이), tcp는 호스트 내 포트까지 연결하며 해당 포트에서 기다리고 응용 프로그램까지 도달
* TCP/IP계층: 4개 계층, 인터넷프로토콜, 운영 체제의 일부로 구현되어 있음 → 이 서비스를 이용하면 됨
  * 전송 방식: TCP(전송제어프로토콜, 에러검출, 재전송 등 데이터 신뢰성 ), UDP(빠른 전송)
* 클라이언트 - 서버
  * 서버: 클라이언트에게 네트워크를 통해 정보나 서비스를 제공하는 컴(hw) 또는 프로그램
    * 웹서버 - iis(ms) -웹 서비스 하는 서버 프로그램 
  * 클라이언트: 네트워크를 통해 서버라는 다른 컴퓨터 시스템 상의 원격서비스에 접속할 수 있는 응용프로그램 또는 사용자 컴퓨터
  * 포트번호:서버의 입장에서 클라이언트 구분
    * 접속된 다수의 응용프로그램 구분 위한 번호, 0~65535(예약된 번호: 0~1023)
    * > 7번: 에코, 13번 DayTime, 21/23: FTP,Telnet, 25: smtp, 80: http
* TCP서버 기본 구조 및 클래스
  * 대기상태(TcpListener) → 접속 요청(TcpClient) → 데이터 전송(NetworkStream)
    * TcpListener: 연결과 TcpClient 객체 생성
    * TcpClient: 데이터 전송
    * 접속 요청 받으면 서버측도 tcpclient객체를 만들어 클라이언트의 tcpclient와 1대1로 붙어서 계속적으로 데이터 주고 받음
    * NetworkStream 통로
  * 다중접속 시, 매 연결/클라이언트 요청 마다 스레드 생성되면서 StreamRead/Write
* UDP서버와 클래스: 기본은 TCP와 같으나, 대기상태는 접속 요청을 받는 게 아니라 바로 데이터 받음
  * 비연결형: IP주소와 포트 번호 알면 데이터 전송 가능(접속요청, 즉 연결을 따로 하지 않음을 의미)
  * UdpClient: 서버와 클 모두에서 사용  /  그룹 처리: UdpClient.JoinMulticast() 사용
* loopback: 컴퓨터환경에서 자기 자신에 접근하는 경우
  * 127.0.0.1
    * OS자체적으로 제공하며, 항상 고정된 자기자신을 가리키는 IP, 예약된 ip주소
    * os에서 가상 지원, 랜카드 등 디바이스 자체를 통과하지 않고 sw적으로 처리됨
  * localhost
    * 호스트네임, 자신의 ip로 직접 접근 시, localhost로 접근하는게 더 빠르고 자원 덜 씀
* cmd명령어
  * ipconfig: 내 컴퓨터의 네트워크 환경(IP정보) 확인 가능
  * ping(Packet INternet Groper): 대상 컴을 향해 일정 크기의 패킷을 보내고 이에 대한 응답을 받아 대상 컴퓨터의 동작 여부나 네트워크 상태 파악 가능
     > ping [IP 또는 도메인]
  * nsLookup: DNS에 질의하여, 도메인의 정보를 조회, 어떤 주소의 실제 IP주소알 수 o
  * netstat: 현재 내 컴에서 연결중이거나 연결대기중인 상태의 port 확인 가능
  * route: 현재 넷상의 모든 연결

2. 네트워크 클래스: 정보 클래스 > 연결 클래스 > 전송 클래스 
(1) 정보 클래스: 클래스지만 구조체 정도 수준
* IPAddress: ip주소 (string 형식-127.0.0.1 <-> 주소의 실체 - long형 값)
  * Parse(string ipString) - long 타입 반환  / ToString() - string 형태로 반환 
* DNS: 도메인 명 <-> ip주소 관리
  * GetHostEntry(IPAddress addr또는 string 호스트명/주소): IPHostEntry형태로 반환
  * GetHostAddresses	:IPAddrress[ ] 배열 형태로 반환( 여러개의 IP갖는 경우)
  (정적static 함수 → 객체 생성 없이 사용 가능)
* IPHostEntry: 도메인명과 ip주소 저장하는 컨테이너 → AddressList / HostName
* IPEndPoint: 목적지 ip주소와 포트번호를 저장 → IPEndPoint(long/IPAddress addr, int port)

(2) 연결 클래스: 실제 연결을 해주는 클래스, 내부적으로 소켓 기반(Winsock)
* TcpListener(IPAddress, int port): 서버와 클 구분할 수 있는 클래스, 클라이언트의 연결 대기
  * LocalEndPoint: 현재 연결된 끝점(end point) - 서버 측
  * Start / Stop:대기 상태 시작, 정지
  * AcceptTcpClient: 클라이언트 요청 대기 및 tcpclient생성(연결수락)
    * > blocking 주의: 연결전까지 대기만 가능, 스레드 상태 변화 불가(진행중인 스레드 마치고 상태변화 적용 되므로)
  * Client.RemoteEndPoint → cast형변환 통해서 IPEndPoint
  * Pending: 보류중인 연결 존재 시
* TcpClient(호스트명, port) - 서버, 클라이언트 모두에
  * Connected: 연결 여부
  * Close(): 연결 해제

(3) 전송 클래스
* NetworkStream: tcp연결에서 데이터 송수신 스트림 - 기본 전달 경로 제공
  * GetStream
  * Read / Write 	⇒ byte[]로 전송: Encoding.ASCII.GetBytes(string str)
  * Close
* StreamReader / Writer(Stream stream)
  * StreamWriter: 문자열 끝에 종결자(\n, \r\n, \r) 붙여씀 ,텍스트파일
    * WriteLine
    * close: 자동으로 가능→ NetworkStream ns = tcpClient.GetStream(); using(StreamWriter sw = new StreamWriter(ns){~~}	// 해당 괄호 끝나면 저절로 close
    * AutoFlush = true;	// writeline한 후 버퍼비우기 
  * StreamReader: streamwriter에서 전달하는 문자열을 종결자 단위로 읽기 //네트워크 스트림으로 하려면 일일이 파스해야해서 불편
    * WriteLine
    * ReadLine(): 종결자 단위로 읽기 //string으로 리턴, 읽을 거 없으면 null
* BinaryWriter/Reader - 이진 파일(임의의 데이터 형 해석<->일반적인 것들은 1바이트 단위형 해석)
  * Write
  * ReadXXX→ ex) ReadString(), ReadInt32(), ReadChar 등
  
3. Socket Programming
  * 송신: 소켓 생성 → 연결 요청 및 연결(connect) → 통신(send/ recv) → 소켓 닫기
  * 수신: 소켓 생성→ ip주소,포트 할당(bind) → 연결 대기(listen)-->연결 승인(accept)--> 통신 → 닫기
* socket: 네트워크 상에서 동작하는 프로그램 간 통신의 종착점(Endpoint)
    * 응용프로그램에서 tcp/ip를 이용하는 창구역할, 통신을 위한 통로
    * 프로그램이 네트워크에서 데이터를 통신할 수 있도록 연결해주는 연결부
  * TCP/IP기반의 Stream방식
  * UDP/IP기반의 Datagram방식
* Socket클래스
    * tcp관련 클래스는 TCP/IP만 지원, 소켓클래스는 IP외에도 다양한 네트워크
    * Available: 읽을 수 있는 데이터의 양 --> Receive할 때 그만큼의 공간만 확보해서 받을 수 있음
    * Receive: 바인딩 된 소켓 정보 받기, 받은 바이트 수 반환
    * RemoteEndPoint: 원격 끝점 -->EndPoint로, cast연산 통해 IPEndPoint(O) / Local도 있음
    * Send: byte[]로만 가능 

==> 소켓 연결 대기 중에는 Form다른 동작x --> 스레드 필요
4. Thread
  * 운영체제의 자원, 프로그램 안에서 독립적으로 실행 가능(멀티태스킹 os)
    * 함수를 스레드에게 실행해달라고 구현해서 줌 --> new Thread(함수명)
> 프로그램(코드& 데이터) → 프로세스(os로부터 할당받은 메모리에 코드와 데이터를 저장, cpu 할당 받아 실행 가능한 상태) → 스레드(프로세스를 할당 받고 코드를 실행) (프로세스:메모리개념, 스레드는 실행 개념)
* 실행: 프로그램 실행(Main→ 주스레드, 싱글 스레드) / 코드에 의해 (thread라는 객체를 이용해서 프로그래머가 실행시킴)
* 스레드 함수의 호출 구조
: 스레드 함수 구현 → 델리게이트 생성과 스레드 함수 설정 → 스레드 생성 → 스레드 실행
    * call function → delegate를 통해 실행 가능 :함수의 원형이 같아야 
> ex) th1.Start(); th2.Start(); Console.WriteLine(“메인종료”); ⇒ 메인종료 먼저뜨고 두 스레드 각각 반복문돌아감
* 주요 속성
  * Name
  * IsAlive
  * IsBackground  // foreground - 주 스레드에 독립적, background: 주스레드와 함께 종료
  * CurrentThread: 현 스레드 반환, 각 스레드마다 부여되는 아이디(gethashcode())를 얻기 위해 많이 사용
* 메서드
  * Start
  * Join: 스레드가 종료될 때 까지 대기
  * Abort: 스레드 중지,이 함수를 호출한 곳의 현재 스레드를 중지→current로 가리켜서 중지 가능 (Thread.CurrentThread.Abort())
  * resume, suspend → 닷넷 2.0 버전 이후부터 사용하지 않는 메서드이며 제거됨
 * 교차 스레드(cross Thread): 컴포넌트 생성한 스레드와 호출한 스레드가 다를 때 발생
    > 해당 스레드를 만든 곳이 아니면 동작x
  * 해결: delegate: 대리자로서 메서드를 다른 메서드의 인수로 전달
* 콜백함수
    * 일반적으로 우리는 시스템에 콜을 해서 함수를 호출, 이와 반대되는 개념이 콜백함수
    * 사용자가 콜해서 시스템이 실행하다가 사용자가 콜백하라고 한거 보고 말해주는것
==> 콜백을 구현할 때 사용하는 것이 대리자
* Delegate(대리자): 대신 수행, 메서드에 대한 참조를 가리키는 형식으로 메서드 간접 호출 가능
    * 함수의 포인터라고 생각하면 편함
    * delegate선언, 그곳에 맞는 콜백함수를 선언하여 처리
    * 프로젝트 내용 중 addText함수: 컨트롤.InvokeRequired발생 시, invoke로 실행
