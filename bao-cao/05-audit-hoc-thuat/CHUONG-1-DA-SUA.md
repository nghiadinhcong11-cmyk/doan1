# TỔNG QUAN ĐỀ TÀI

## Lý do chọn đề tài

### Những thách thức trong quản lý và vận hành nhà hàng đa chi nhánh

Hoạt động của một nhà hàng phục vụ tại bàn liên kết nhiều công việc: quản lý thực đơn và bàn, tiếp nhận yêu cầu gọi món, chuyển món đến bếp, theo dõi chế biến, thanh toán và tổng hợp dữ liệu kinh doanh. Trong một tình huống vận hành nhiều chi nhánh, mỗi cơ sở có nhân sự, bàn, đơn hàng và tồn kho riêng, trong khi bộ phận quản trị vẫn cần một cách nhìn thống nhất. Bài toán của đề tài vì vậy không chỉ là lưu dữ liệu tập trung mà còn là xác định dữ liệu và quyền thao tác nào thuộc về từng chi nhánh.

Nếu thông tin gọi món được ghi nhận rời rạc giữa khách, nhân viên phục vụ, quầy thu ngân và bếp, các bên có thể phải nhập lại cùng một dữ liệu hoặc khó xác định phần món đã được xử lý. Trường hợp khách gọi thêm sau khi một đợt món đã chuyển xuống bếp cho thấy tầm quan trọng của trạng thái đơn hàng và trạng thái yêu cầu chế biến: hệ thống phải phân biệt món đã gửi với món bổ sung. Tương tự, thanh toán cần dựa trên đơn hàng đã lưu và các quy tắc tính tiền ở phía máy chủ để số liệu ở POS, hóa đơn và báo cáo có cùng căn cứ.

Quản lý đa chi nhánh còn đặt ra yêu cầu về phạm vi truy cập. Quản trị viên có thể cần xem dữ liệu toàn chuỗi, nhưng người quản lý chi nhánh và nhân sự vận hành chỉ nên thao tác trong phạm vi được cấp ở từng chức năng. Vai trò của người dùng, chi nhánh được gán và chi nhánh của đối tượng nghiệp vụ phải được kiểm tra ở phía backend. Giao diện có thể hỗ trợ điều hướng theo vai trò nhưng không thay thế được việc xác thực, phân quyền và kiểm tra đối tượng tại API hoặc dịch vụ.

Các nghiệp vụ bổ trợ như quản lý nhân sự, lịch làm việc, chấm công, khách hàng thân thiết, kho và phân tích kinh doanh cũng sử dụng dữ liệu phát sinh từ hoạt động hằng ngày. Khi các nhóm chức năng này được tổ chức trên cùng một nền tảng, thông tin có thể được truy vết theo chi nhánh và theo quy trình nghiệp vụ. Đề tài chọn bài toán quản lý nhà hàng đa chi nhánh để nghiên cứu cách thiết kế một hệ thống liên thông, đồng thời giữ rõ ranh giới dữ liệu và trách nhiệm của mỗi thành phần.

### Nhu cầu số hóa quy trình và tương tác có kiểm soát

Trong phạm vi đề tài, số hóa được hiểu là tổ chức thông tin và luồng xử lý của một tình huống nhà hàng trên phần mềm. Thực đơn QR cho phép khách truy cập bằng trình duyệt và gửi lựa chọn tại bàn; POS tiếp nhận và xử lý đơn; KDS hỗ trợ bếp theo dõi yêu cầu chế biến; thanh toán và chương trình khách hàng thân thiết sử dụng trạng thái đơn đã được ghi nhận. Giá trị thiết kế nằm ở sự liên kết giữa các bước này, không chỉ ở việc thay thực đơn giấy bằng một trang web.

Một hệ thống có nhiều màn hình quản trị cũng tạo nhu cầu tra cứu thông tin thuận tiện hơn. Người dùng có thể nêu yêu cầu về doanh thu, đơn hàng hoặc dữ liệu nghiệp vụ bằng câu ngôn ngữ tự nhiên; thành phần trợ lý sau đó lựa chọn công cụ mà ứng dụng cung cấp để xử lý yêu cầu. Tuy nhiên, lời gọi công cụ có thể đọc dữ liệu nhạy cảm hoặc thay đổi giá, trạng thái đơn và các thông tin khác. Vì vậy, cơ chế điều phối phải gắn với ngữ cảnh người dùng, kiểm tra quyền và tham số, đồng thời yêu cầu xác nhận đối với thao tác nhạy cảm khi quy trình nghiệp vụ quy định.

Mã nguồn hiện tại sử dụng provider mô phỏng có phản hồi xác định để khảo sát cơ chế điều phối công cụ. Cách cài đặt này cho phép kiểm tra luồng giữa trợ lý, registry, dịch vụ nghiệp vụ và lớp phân quyền mà không đồng nghĩa với việc đã tích hợp hay đánh giá một mô hình ngôn ngữ lớn vận hành trực tiếp. Chất lượng hiểu câu hỏi tự nhiên, chọn công cụ và tổng hợp câu trả lời của một mô hình thật là vấn đề đánh giá riêng. Từ bài toán vận hành và giới hạn đó, đề tài “Hệ thống quản lý nhà hàng đa chi nhánh tích hợp Trợ lý ảo AI Agent” được lựa chọn như một nghiên cứu ứng dụng về liên thông nghiệp vụ và tương tác qua công cụ có kiểm soát.

## Mục tiêu nghiên cứu

### Mục tiêu tổng quát

Mục tiêu của đề tài là phân tích, thiết kế và xây dựng một hệ thống web hỗ trợ quản lý nhà hàng đa chi nhánh, trong đó quy trình đặt món, xử lý bếp, thanh toán và các dữ liệu quản trị được liên kết thông qua backend tập trung. Hệ thống hướng tới việc phân định quyền theo vai trò và chi nhánh, giữ các phép tính tài chính tại nguồn dữ liệu có thẩm quyền và cung cấp một lớp tương tác trợ lý có thể gọi các công cụ nghiệp vụ trong phạm vi được phép.

Thành phần trợ lý được đặt trong kiến trúc gồm bộ điều phối, danh mục công cụ và lớp kiểm soát quyền. Ở phiên bản mã được khảo sát, provider mô phỏng được dùng để nghiên cứu cách nhận yêu cầu, tạo lời gọi công cụ, xử lý kết quả và dừng hoặc xác nhận thao tác. Mục tiêu này là kiểm tra cơ chế tích hợp phần mềm; đề tài không đặt mục tiêu chứng minh năng lực suy luận hay hiệu quả của một LLM thực tế khi chưa có provider và bộ đánh giá tương ứng.

### Mục tiêu cụ thể

Trước hết, đề tài hướng tới mô hình hóa các tác nhân, đối tượng dữ liệu và quy trình của nhà hàng nhiều chi nhánh. Mô hình cần thể hiện mối quan hệ giữa chi nhánh, khu vực, bàn, nhân viên, khách hàng, sản phẩm, đơn hàng, yêu cầu chế biến và các dữ liệu quản trị; đồng thời cho phép xác định phạm vi truy cập theo từng chức năng.

Tiếp theo, đề tài hướng tới một luồng phục vụ có thể theo dõi từ thực đơn QR và thao tác tại POS đến KDS và thanh toán. Đơn hàng, trạng thái món gửi bếp và số tiền thanh toán cần được xử lý nhất quán ở backend. Những nghiệp vụ hỗ trợ như đặt bàn, khách hàng thân thiết, quản lý nhân sự và chấm công, kho, chi phí, báo cáo và Business Insights được xem xét trong cùng mô hình dữ liệu, với mức độ cài đặt và kiểm chứng trình bày theo từng module.

Một mục tiêu khác là tổ chức hai giao diện web phù hợp với nhóm sử dụng. Ứng dụng admin-web phục vụ quản trị và vận hành, bao gồm POS và KDS; ứng dụng customer-web phục vụ thực đơn số, đặt món và các tương tác dành cho khách. Các vai trò xác thực gồm Admin, Manager, Employee, Cashier, Kitchen và Customer; Guest là ngữ cảnh truy cập khách vãng lai qua phiên QR hoặc luồng dành cho khách, không phải một vai trò JWT độc lập. Quyền Manager được xác định theo chức năng và chi nhánh được gán, không mặc nhiên tương đương quyền Admin.

Đối với trợ lý, mục tiêu là xác định đường thực thi từ yêu cầu của người dùng đến công cụ nghiệp vụ, kiểm tra quyền và ngữ cảnh, xác thực đối số, đồng thời phân biệt truy vấn chỉ đọc với thao tác thay đổi dữ liệu. Các thao tác nhạy cảm cần có điểm dừng hoặc xác nhận phù hợp. Việc đánh giá tập trung vào tính đúng đắn của ranh giới phần mềm trong phạm vi kiểm chứng có được, không dùng phản hồi mô phỏng làm thước đo chất lượng ngôn ngữ của AI thật.

Cuối cùng, đề tài hướng tới việc đối chiếu yêu cầu, thiết kế và cài đặt bằng các hình thức kiểm tra có thể truy vết. Mục tiêu đánh giá là xác định nội dung nào được chứng minh bằng mã, build, kiểm thử tự động hay thao tác thực tế và nội dung nào còn thiếu minh chứng. Không suy từ sự tồn tại của một kịch bản kiểm thử rằng kịch bản đó đã được chạy.

## Nhiệm vụ nghiên cứu

Để theo đuổi các mục tiêu trên, trước hết đề tài cần nghiên cứu cơ sở lý thuyết và tài liệu kỹ thuật về hệ thống quản lý nhà hàng, ứng dụng web, cơ sở dữ liệu quan hệ, xác thực và phân quyền, giao tiếp thời gian thực, cùng cơ chế điều phối và gọi công cụ của trợ lý. Tài liệu này cung cấp thuật ngữ và tiêu chí để phân tích bài toán, không thay thế việc kiểm tra sản phẩm.

Nhiệm vụ tiếp theo là phân tích quy trình nghiệp vụ và xác định yêu cầu. Công việc bao gồm nhận diện các tác nhân, mô tả luồng đặt món QR/POS, gửi bếp, thanh toán và các nghiệp vụ quản trị; xác định trạng thái, dữ liệu đầu vào, điều kiện lỗi, phạm vi chi nhánh và quyền thao tác. Các yêu cầu chức năng cần đi cùng những yêu cầu về bảo mật, toàn vẹn và khả năng sử dụng.

Trên cơ sở yêu cầu, đề tài cần lập mô hình Use Case, hoạt động, trình tự, lớp miền nghiệp vụ và ERD; thiết kế kiến trúc cho backend, hai giao diện web, cơ sở dữ liệu và kênh thông báo thời gian thực. Mô hình dữ liệu phải làm rõ quan hệ và ràng buộc thay vì suy đoán rằng mọi trường có hậu tố Id đều là khóa ngoại.

Nhiệm vụ cài đặt là tổ chức các dịch vụ nghiệp vụ, API và giao diện theo thiết kế, nối luồng QR/POS/KDS/thanh toán với các module hỗ trợ trong phạm vi sản phẩm. Với trợ lý, công việc là xây dựng provider abstraction, bộ điều phối, danh mục công cụ và kiểm tra quyền để công cụ đi qua dịch vụ ứng dụng. Provider mô phỏng là phương tiện khảo sát cơ chế hiện có, không phải bằng chứng đã huấn luyện hoặc tích hợp LLM thật.

Nhiệm vụ đánh giá là xây dựng ca kiểm tra từ các yêu cầu quan trọng, thực thi những kiểm tra khả thi trong môi trường độc lập, ghi lại đầu vào, kết quả kỳ vọng, kết quả thực tế và bằng chứng. Các phiên kiểm thử lịch sử, nếu được sử dụng, phải ghi thời điểm và phiên bản mã. Trên cơ sở đó, đề tài đối chiếu kết quả với mục tiêu, nêu giới hạn và đề xuất các bước kiểm chứng tiếp theo.

## Đối tượng và phạm vi nghiên cứu

### Đối tượng nghiên cứu

Đối tượng nghiên cứu gồm các quy trình vận hành nhà hàng phục vụ tại bàn, cách tổ chức dữ liệu trong một hệ thống nhiều chi nhánh và các cơ chế phần mềm để kết nối người dùng với nghiệp vụ. Các quy trình trọng tâm là quản lý thực đơn và bàn, tiếp nhận đơn, chuyển yêu cầu chế biến, thanh toán, chăm sóc khách hàng và theo dõi hoạt động quản trị.

Đề tài xem xét hai nhóm sử dụng: nhân sự/quản trị nhà hàng và khách. Ở nhóm nội bộ, vai trò của Admin, Manager, Employee, Cashier và Kitchen tạo ra các nhu cầu thao tác khác nhau. Ở nhóm khách, Customer sử dụng tài khoản còn Guest tương tác theo ngữ cảnh phiên khách. Việc mô hình hóa vai trò phải đi kèm phạm vi của đối tượng nghiệp vụ và chi nhánh, bởi cùng một chức năng có thể cho phép các mức truy cập khác nhau.

Đối với trợ lý, đối tượng nghiên cứu là cơ chế chuyển yêu cầu thành lời gọi công cụ có cấu trúc, việc xây dựng ngữ cảnh, lọc công cụ theo quyền, kiểm tra kết quả và kiểm soát thao tác ghi. Đề tài tìm hiểu nguyên lý tương tác với mô hình ngôn ngữ như cơ sở lý thuyết, nhưng trường hợp cài đặt hiện tại dùng provider mô phỏng xác định; hai phạm vi này được phân biệt khi đánh giá.

### Phạm vi nghiên cứu

Về nghiệp vụ, đề tài tập trung vào quản lý nhà hàng nhiều chi nhánh với luồng phục vụ tại bàn: khách truy cập thực đơn QR, gửi yêu cầu, nhân sự tiếp nhận và xử lý tại POS, bếp theo dõi yêu cầu trên KDS, thu ngân ghi nhận thanh toán. Phạm vi hỗ trợ gồm quản lý chi nhánh, bàn, sản phẩm, khách hàng, Loyalty, đặt bàn, nhân sự, lịch làm việc, chấm công, kho, chi phí, báo cáo và Business Insights theo các thành phần được cài đặt. Kho có danh mục mặt hàng, tồn theo chi nhánh, phiếu nhập/xuất và lịch sử biến động; mức độ hoạt động thực tế của các luồng này cần được xác minh riêng.

Sự hiện diện của BasicSalary và EmployeeType trong hồ sơ nhân viên không đồng nghĩa với việc có module tính lương. Đề tài không đưa Payroll, kế toán tổng hợp hay quản trị chuỗi cung ứng toàn diện vào phạm vi sản phẩm khi thiếu đường cài đặt tương ứng. Tương tự, việc ghi nhận chuyển khoản trong ứng dụng không tự chứng minh có webhook ngân hàng hoặc xác nhận giao dịch tự động.

Về công nghệ, phạm vi là backend ASP.NET Core 8 sử dụng Entity Framework Core và cấu hình PostgreSQL, cùng hai ứng dụng React/TypeScript/Vite: admin-web cho quản trị và vận hành, customer-web cho khách. POS và KDS là các màn hình của admin-web. SignalR phục vụ các thông báo thời gian thực theo cơ chế đã cài đặt. Việc mã nguồn có cấu hình kết nối PostgreSQL không chứng minh schema của một cơ sở dữ liệu triển khai đã được kiểm tra trong nghiên cứu này.

Về AI, phạm vi cài đặt hiện tại là điều phối công cụ qua IAiProvider, AiOrchestrator, AiToolRegistry, AiPermissionService và các dịch vụ nghiệp vụ. Đăng ký dịch vụ trỏ tới MockAiProvider, một provider mô phỏng chỉ được cấu hình cho môi trường Development và xử lý một tập ý định giới hạn bằng quy tắc xác định. Vì vậy, nghiên cứu có thể đánh giá luồng gọi công cụ, từ chối quyền và xác nhận theo điều kiện phần mềm; chưa thể kết luận về chất lượng hiểu ngôn ngữ hoặc hiệu năng của Gemini, OpenAI hay một LLM live.

Về đánh giá, các hoạt động được giới hạn bởi bằng chứng có thể tái kiểm tra trên phiên bản mã được nêu. Build chỉ cho biết khả năng biên dịch/đóng gói; đọc mã cho biết cấu trúc và điều kiện cài đặt; kiểm thử service hoặc checklist thủ công chỉ chứng minh những ca thực sự được chạy và ghi nhận. Kết quả kiểm thử của phiên cũ không được chuyển thành kết quả hiện tại. Đề tài chưa đặt mục tiêu chứng minh hiệu quả kinh tế, độ hài lòng người dùng, khả năng chịu tải quy mô lớn hay mức cải thiện vận hành khi chưa có dữ liệu thực nghiệm phù hợp.

## Phương pháp nghiên cứu

### Phương pháp nghiên cứu tài liệu

Đề tài sử dụng giáo trình và tài liệu kỹ thuật có thể truy vết để thống nhất khái niệm về POS, KDS, cơ sở dữ liệu quan hệ, UML/ERD, kiến trúc web, JWT, phân quyền, SignalR và cơ chế gọi công cụ. Phần lý thuyết về mô hình ngôn ngữ giúp xác định khả năng và giới hạn của một lớp tương tác AI; nó không được dùng làm bằng chứng rằng sản phẩm đã kết nối một mô hình cụ thể. Các nhận định về xu hướng thị trường hoặc mức cải thiện hiệu quả chỉ được giữ khi có tài liệu tham khảo xác minh được; nếu không, chúng được trình bày như động cơ thiết kế cho tình huống nghiên cứu.

### Phương pháp phân tích yêu cầu và nghiệp vụ

Đề tài phân tích các tình huống nghiệp vụ theo chuỗi sự kiện và theo tác nhân. Với một đơn hàng, phương pháp này lần theo dữ liệu từ lựa chọn của khách, thao tác tiếp nhận, đợt gửi món đến bếp, cập nhật trạng thái và thanh toán. Ở cấp quản trị, việc phân tích tập trung vào chi nhánh sở hữu dữ liệu, vai trò được phép thực hiện thao tác và điều kiện toàn vẹn của đối tượng liên quan.

Từ từng tình huống, đề tài tách yêu cầu chức năng khỏi các ràng buộc phi chức năng như bảo mật, tính nhất quán và khả năng truy vết. Những trường hợp không hợp lệ cũng được đặt ra cùng luồng chính, chẳng hạn mã QR không hợp lệ, truy cập ngoài chi nhánh, số lượng món sai hoặc thanh toán không khớp tổng tiền lưu ở backend. Phân tích này dựa trên mô hình tình huống và đối chiếu mã nguồn, không được trình bày như kết quả của một cuộc khảo sát đại diện hay phỏng vấn thực địa.

### Phương pháp mô hình hóa và thiết kế hệ thống

Use Case được sử dụng để xác định tác nhân và mục tiêu tương tác; biểu đồ hoạt động làm rõ trình tự và nhánh điều kiện; biểu đồ trình tự biểu diễn thông điệp giữa giao diện, API, dịch vụ và thành phần thời gian thực. Biểu đồ lớp và ERD thể hiện đối tượng miền nghiệp vụ, thuộc tính và quan hệ dữ liệu. Các mô hình này được kiểm tra lại với entity, mapping và hợp đồng API để giảm khoảng cách giữa thiết kế và cài đặt.

Đối với trợ lý, phương pháp thiết kế phân tách vai trò của provider, bộ điều phối, registry, lớp kiểm tra quyền và dịch vụ ứng dụng. Một lời gọi do provider trả về chỉ là đề xuất đầu vào; hệ thống phải xác thực tên công cụ, đối số, vai trò, chi nhánh và điều kiện xác nhận trước khi thực thi. Cách phân tách này cho phép khảo sát cơ chế phần mềm mà không gán quyền nghiệp vụ cho provider.

### Phương pháp phát triển phần mềm

Việc cài đặt được tổ chức theo các tầng trách nhiệm: frontend thu nhận thao tác và hiển thị trạng thái; API xác thực yêu cầu; dịch vụ ứng dụng thực thi quy tắc nghiệp vụ; tầng dữ liệu lưu và truy xuất đối tượng. Với các giá trị tài chính, phương pháp triển khai đặt phép tính và kiểm tra thanh toán tại backend. Với dữ liệu nhiều chi nhánh, từng đường thao tác cần xác định chi nhánh từ ngữ cảnh có thẩm quyền thay vì tin vào lựa chọn hiển thị ở frontend.

Các nhóm chức năng được tích hợp theo luồng sử dụng thực tế. Ví dụ, sửa đơn hàng cần xét đồng thời giá trị tài chính, trạng thái bếp và trạng thái thanh toán; quản lý kho cần xét tồn theo chi nhánh và lịch sử nhập/xuất; trợ lý dùng lại dịch vụ nghiệp vụ thay vì tự xây dựng một quy tắc tính tiền hoặc cập nhật dữ liệu riêng. Những ví dụ này là nguyên tắc thiết kế và tiêu chí đối chiếu, không tự khẳng định mọi trường hợp đã được kiểm thử runtime.

### Phương pháp kiểm thử và đánh giá

Ca kiểm thử được thiết kế từ yêu cầu và các điểm dễ sai của luồng nghiệp vụ: quyền và phạm vi chi nhánh, trạng thái đơn/bếp, tính tiền, thanh toán, kho và gọi công cụ của trợ lý. Với checklist thủ công, mỗi ca cần ghi điều kiện trước, dữ liệu nhập, bước thao tác, kết quả kỳ vọng, kết quả quan sát và minh chứng như phản hồi API hoặc ảnh màn hình có thời điểm. Chỉ sau khi thực thi và đối chiếu mới được ghi trạng thái đạt hoặc không đạt.

Các phép kiểm tra độc lập như build hoặc test tự động, khi thực sự được chạy, được ghi kèm lệnh, thời điểm, mã nguồn và kết quả. Báo cáo tách kết quả lịch sử khỏi checkout hiện tại, đồng thời phân biệt test service với kiểm thử HTTP, trình duyệt, SignalR và PostgreSQL. Trợ lý dùng provider mô phỏng có thể được kiểm tra ở ranh giới điều phối và quyền; phép đo khả năng chọn công cụ của LLM thật đòi hỏi một tập câu hỏi, đáp án và provider thực tế riêng.

Việc đánh giá cuối cùng đối chiếu mục tiêu với yêu cầu, mô hình thiết kế, mã cài đặt và những minh chứng đã thu được. Nội dung chưa chạy hoặc chưa thể tái lập được ghi là giới hạn kiểm chứng, thay vì được suy thành kết quả đạt. Cách này giúp xác định chính xác phần đã được hỗ trợ bởi bằng chứng và công việc cần thực hiện trong một lần thử nghiệm tiếp theo.

## Ý nghĩa khoa học và thực tiễn của đề tài

### Ý nghĩa khoa học

Đây là đề tài ứng dụng kiến thức về phân tích thiết kế hệ thống thông tin vào một bài toán có nhiều vai trò, chi nhánh và chuỗi xử lý phụ thuộc nhau. Việc mô hình hóa luồng QR/POS/KDS/thanh toán cùng các đối tượng nhân sự, khách hàng, kho và báo cáo làm rõ cách nối yêu cầu nghiệp vụ với kiến trúc và cấu trúc dữ liệu. Các biểu đồ và ràng buộc được dùng để giải thích lý do phân chia trách nhiệm giữa giao diện, dịch vụ và tầng dữ liệu.

Ở thành phần trợ lý, đóng góp phương pháp nằm ở cách đặt tương tác qua công cụ trong ranh giới xác thực và nghiệp vụ của ứng dụng. Trường hợp provider mô phỏng giúp khảo sát đường đi của yêu cầu, việc lọc công cụ, kiểm tra quyền, đối số và điểm xác nhận, qua đó nêu được điều kiện cần trước khi kết nối một mô hình thật. Đề tài không đề xuất thuật toán AI mới, không chứng minh một kiến trúc tối ưu và không công bố độ chính xác của mô hình ngôn ngữ khi chưa có thực nghiệm tương ứng.

### Ý nghĩa thực tiễn

Sản phẩm hướng tới một nền tảng web cho phép nhân sự và khách hàng tham gia cùng một luồng phục vụ có dữ liệu liên kết. Thực đơn QR, POS và KDS cung cấp những điểm tương tác khác nhau cho khách, quầy và bếp; backend duy trì đơn hàng và điều kiện thanh toán. Các chức năng quản trị về chi nhánh, nhân sự, khách hàng, Loyalty, kho, chi phí và báo cáo tạo cơ sở để tổ chức thông tin theo phạm vi quản lý.

Lớp trợ lý cung cấp một hướng tiếp cận khác đối với công cụ nghiệp vụ, đồng thời cho thấy việc kiểm soát quyền phải nằm ở ứng dụng. Trong phạm vi phiên bản hiện tại, giá trị thực tiễn được trình bày như khả năng khảo sát và trình diễn quy trình tích hợp, không phải hiệu quả đã đo trên người dùng hoặc bằng một dịch vụ AI thương mại. Những lợi ích như giảm thời gian phục vụ, giảm sai sót hay tăng doanh thu chỉ là giả thuyết cần kiểm nghiệm bằng dữ liệu vận hành và nghiên cứu người dùng ở giai đoạn sau.
