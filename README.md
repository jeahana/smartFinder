# SmartFinder 🔍

**SmartFinder**는 대용량 소스 코드 및 파일 시스템에서 강력하고 빠른 다중 키워드 검색을 지원하는 고성능 **WPF 기반 Windows 데스크톱 애플리케이션**입니다. 

개발자의 생산성을 극대화하기 위해 다중 스레드 기반 검색 엔진, 정교한 엑셀식 필터링 그리드, 구문 강조(Syntax Highlighting)가 내장된 코드 뷰어, 그리고 유연한 외부 에디터 연동 기능을 완벽히 통합하여 구현하였습니다.

---

## 🌟 주요 핵심 기능

### 1. 초고속 다중 키워드 검색 엔진
* **멀티 키워드 줄바꿈 검색**: 검색어를 엔터(Enter) 단위로 줄바꿈하여 여러 개 입력하고 동시에 검색할 수 있습니다.
* **유연한 연산자 지원**: 입력한 여러 키워드에 대해 **AND(모두 포함)** 또는 **OR(하나라도 포함)** 매칭 방식을 선택 가능합니다.
* **정규표현식(Regex) & 대소문자 구분**: 정밀한 패턴 매칭을 위해 C# 정규식 엔진 및 대소문자 매칭 조건(Match Case)을 제공합니다.
* **하위 폴더 탐색**: 하위 디렉토리(Subdirectories) 재귀 탐색 여부를 손쉽게 제어합니다.

### 2. 엑셀 방식(Excel-like) 그리드 필터링 & 정렬
* **열 필터 팝업Dropdown**: 각 컬럼마다 엑셀과 동일한 검색, 전체 선택/해제, 고유 값 목록 체크 필터링이 내장되어 대량의 결과도 실시간 제어할 수 있습니다.
* **필터 팝업 리사이즈**: 데이터가 길거나 많을 때 마우스 드래그로 필터 창 크기를 자유롭게 확대/축소할 수 있는 대각선 리사이즈 그립을 지원합니다.
* **스마트 컬럼 레이아웃**:
  * **검색어 / Path / File Name / Ext / Line / Line Text / Note** 구조의 전문적인 데이터 그리드 컬럼 배치.
  * Ext(확장자) 컬럼의 불필요한 점(`.`) 표기 제거로 가독성 향상.
  * **Line Text 컬럼 Stretch**: 코드 줄의 텍스트 영역을 화면 크기에 맞게 자동으로 늘려 가독성을 극대화하였습니다.
* **영역별 정렬 자동화**: 검색 완료 시 **디렉토리 ➔ 파일명 ➔ 줄 번호 ➔ 검색 키워드** 순서로 결과가 논리적으로 정렬되어 일목요연하게 표시됩니다.

### 3. 멀티 탭 시트 결과 보관
* **결과 보관(Keep Results) 기능**: 체크 시 새로운 검색을 수행할 때마다 기존 탭이 유지되고 `"검색 1"`, `"검색 2"`와 같은 순차적인 결과 시트가 새롭게 자동 생성됩니다.
* **원클릭 탭 닫기 & 컨텍스트 메뉴**: 마우스 ✕ 버튼 또는 탭 우클릭의 "시트 삭제" 기능을 지원하며, 완벽히 정제된 WPF 바인딩 라이프사이클로 설계되어 에러가 전혀 발생하지 않습니다.

### 4. 내장형 파일 인스펙터 (코드 뷰어)
* **Double-Click 연동**: 그리드 행을 더블클릭하거나 선택하면, 프로그램 내부 우측의 전용 파일 뷰어로 파일 내 해당 줄의 소스 코드가 즉시 스크롤되어 표시됩니다.
* **강력한 AvalonEdit 구문 강조**: 
  * C#, Java, C++, XML, HTML, Python 등의 표준 언어는 물론, **React/JS/TS/JSX/TSX 전용 Syntax Highlighting** 라이브러리가 커스텀 빌드되어 내장되어 있습니다.
  * 다크 모드 및 라이트 모드 테마에 유기적으로 호환되어 동작합니다.

### 5. 풍부한 다이내믹 테마 시스템
* **Light / Dark 대분류 테마**: 원클릭으로 밝은 모드와 어두운 모드를 전환할 수 있습니다.
* **수십 종의 프리미엄 서브 테마**:
  * **Dark 계열**: SmartFinder Dark, Default Dark, Nord Dark, Midnight Black, Deep Black, Monokai, Obsidian, Solarized Dark, Ruby Blue, Twilight, Zenburn, Choco, Bespin 등.
  * **Light 계열**: SmartFinder Light, Default Light, Warm Sepia, Ice Blue, Solarized Light, Hello Kitty, Khaki 등.
  * 각 테마는 그리드, 텍스트 박스, 헤더, 팝업, 코드 뷰어 전역에 실시간으로 세련된 그래디언트 및 색조 조합으로 적용됩니다.

### 6. 업무 효율 극대화 유틸리티
* **Excel Down (CSV Export)**: 현재 활성화된 검색 결과 탭을 마이크로소프트 엑셀 호환 CSV 형식 파일로 빠르게 다운로드합니다.
* **Excel Upload (CSV Import)**: 이전에 내보냈던 CSV 파일을 다시 불러와 그리드 상에 그대로 복원 및 조회할 수 있습니다.
* **전용 외부 에디터 연동**: Preferences(환경설정)에서 VS Code, Notepad++, Sublime Text 등의 실행 경로 및 인수 매개변수 패턴(예: `"%path%\%file_name%" -n%line_no%`)을 설정하면 파일 더블클릭 시 자동으로 외부 선호 편집기 내 해당 행이 직접 활성화됩니다.

---

## 🛠️ 개발 및 빌드 환경

* **프레임워크**: `.NET 10.0 Windows (WPF)`
* **언어 사양**: `C# 13`
* **주요 라이브러리**:
  * [CommunityToolkit.Mvvm (MVVM Toolkit)](https://github.com/CommunityToolkit/dotnet) - 클린 아키텍처 상태 바인딩 및 Command 구현
  * [AvalonEdit](https://github.com/icsharpcode/AvalonEdit) - 고성능 코드 인스펙터 뷰어 및 구문 강조
  * [ClosedXML](https://github.com/ClosedXML/ClosedXML) - 고급 엑셀(XLSX/CSV) 다운로드 및 처리
* **인코딩**: `UTF-8 (BOM)`

---

## 🚀 실행 및 빌드 방법

### 1. 콘솔을 통한 다이렉트 빌드
프로젝트 루트 폴더에서 다음 .NET CLI 명령을 통해 간편하게 컴파일할 수 있습니다:
```bash
# Debug 모드로 빌드
dotnet build -c Debug

# Release 모드로 실행 파일 배포 빌드
dotnet build -c Release
```

### 2. 빌드 배치 스크립트 실행
인코딩 손실이나 윈도우 환경에 안전하도록 UTF-8(BOM) 및 CP949 양쪽 콘솔을 모두 배려해 작성된 `build.bat` 배치 파일이 제공됩니다:
```cmd
# Windows CMD 혹은 PowerShell 환경에서 실행
build.bat
```
실행 결과 생성물은 `\bin\Debug\net10.0-windows\` 하위에 위치하게 됩니다.

---

## 📂 프로젝트 구조 안내

* [MainWindow.xaml](file:///c:/workproject/smartFinder/MainWindow.xaml): 정교한 리사이즈 그립, 스크롤바 상시 표시, 다크/라이트 테마가 정의된 메인 윈도우 UI 마크업
* [MainWindow.xaml.cs](file:///c:/workproject/smartFinder/MainWindow.xaml.cs): 창 위치 복원, AvalonEdit 컨트롤 초기화 및 React 커스텀 하이라이팅 연동 로직
* [ViewModels/MainViewModel.cs](file:///c:/workproject/smartFinder/ViewModels/MainViewModel.cs): 상태 변경 이벤트, 검색 스레딩 라이프사이클 관리, 엑셀 입출력, 클린 `"검색 X"` 시트 카운팅 처리가 담긴 핵심 뷰모델
* [Models/SearchTab.cs](file:///c:/workproject/smartFinder/Models/SearchTab.cs): 엑셀식 열 필터링 엔진, ListBox 고유 값 바인딩 및 컬렉션 필터링 뷰 콜백 로직
* [Services/FileSearchService.cs](file:///c:/workproject/smartFinder/Services/FileSearchService.cs): 멀티스레드 기반 디렉토리 비동기식 고성능 검색 파이프라인
* [React-Mode.xshd](file:///c:/workproject/smartFinder/React-Mode.xshd): 프론트엔드 최신 구문 하이라이팅을 위한 커스텀 XML 하이라이팅 리소스 스키마

