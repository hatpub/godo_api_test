// 바인딩 리스트
private BindingList<GODO_LIST> listdata;
// 사은품 리스트
BindingList<GODO_GIFT_LIST> giftdata;
        
// 고도몰 API 연결 Keys
public static string public_key = "";
public static string secret_key = "";

public static string GetRequestResult(string openApiUrl, Dictionary<string, string> paramDic)
{
    string callUrl = openApiUrl;
    string postData = string.Format("partner_key={0}&key={1}", public_key, secret_key);

    foreach (KeyValuePair<string, string> param in paramDic)
    {
        postData += string.Format("&{0}={1}", param.Key, param.Value);
    }

    try
    {
        HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(callUrl);
        byte[] sendData = UTF8Encoding.UTF8.GetBytes(postData);
        httpWebRequest.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
        httpWebRequest.Method = "POST";
        httpWebRequest.ContentLength = sendData.Length;
        Stream requestStream = httpWebRequest.GetRequestStream();
        requestStream.Write(sendData, 0, sendData.Length);
        requestStream.Close();
        HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
        StreamReader streamReader = new StreamReader(httpWebResponse.GetResponseStream(), Encoding.GetEncoding("UTF-8"));
        string return_stm = streamReader.ReadToEnd();
        streamReader.Close();
        httpWebResponse.Close();
        return return_stm;
    }
    catch (Exception ex)
    {
        MessageBox.Show("일시적인 오류로 API를 불러오지 못했습니다.");
        return "";
    }

}

// API Load Button
private void Api_Load()
{
    this.splashScreenManager1.ShowWaitForm();

    string sno;                 //상품고유번호
    string contract_no;         //주문번호
    string company;             //주문자 이름
    string contract_date;       //주문일자(여기서는 업로드 일자)
    string order_nm;            //회원명
    string order_email;         //주문자 이메일
    string order_phone;         //주문자 핸드폰 번호
    string order_nm2;           //수취인 이름
    string order_phone2;        //수취인 핸드폰 번호
    string memo;                //주문시 남기는글
    string post;                //우편번호  
    string recipient_addr1;     //수취인 주소
    string recipient_addr2;     //수취인 나머지 주소
    string country;             //국가코드
    string country_eng;         //국가명(영)
    string commodity;           //품목코드
    string commodity_eng;       //품목명(영)
    string unit;                //중량 코드
    string kg;                  //중량
    string qty;                 //수량
    string price;               //판매가
    string o_price;             //옵션 금액

    listdata = new BindingList<GODO_LIST>();
    giftdata = new BindingList<GODO_GIFT_LIST>();

    string realServer = "https://openhub.godo.co.kr/godomall5/order/Order_Search.php";
    string sandBox = "http://sbopenhub.godo.co.kr/godomall5/order/Order_Search.php";

    Dictionary<string, string> paramDic = new Dictionary<string, string>();

    DateTime today = DateTime.Now;
    DateTime answer = today.AddDays(-30);

    paramDic.Add("dateType", "order");
    paramDic.Add("startDate", answer.ToString("yyyy-MM-dd"));
    paramDic.Add("endDate", today.ToString("yyyy-MM-dd"));
    paramDic.Add("orderStatus", "g1");

    string res = GetRequestResult(realServer, paramDic);

    // 만약 받아오는 값이 없다면 return
    if (res == "" || res == null)
        return;

    //MessageBox.Show(res);
    XmlDocument m_xDoc = new XmlDocument();
    m_xDoc.LoadXml(res);

    // 회신 코드가 정상(000) 이 아닐경우 메시지 남기고 리턴
    XmlNode xNode = m_xDoc.SelectSingleNode("/data/header");
    if (xNode["code"].InnerText != "000")
    {
        MessageBox.Show(xNode["msg"].InnerText, "Error Code " + xNode["code"].InnerText);
        return;
    }

    XmlNodeList xmlList = m_xDoc.SelectNodes("/data/return/order_data");
    foreach (XmlNode xnl in xmlList)
    {
        contract_no = xnl["orderNo"].InnerText;
        company = xnl["orderInfoData"]["orderName"].InnerText.Replace(@"\", "");
        contract_date = xnl["orderDate"].InnerText;
        order_nm = xnl["orderInfoData"]["orderName"].InnerText.Replace(@"\", "");
        order_email = xnl["orderInfoData"]["orderEmail"].InnerText;
        order_phone = xnl["orderInfoData"]["orderCellPhone"].InnerText;
        order_nm2 = xnl["orderInfoData"]["receiverName"].InnerText.Replace(@"\", "");
        order_phone2 = xnl["orderInfoData"]["receiverCellPhone"].InnerText;
        memo = xnl["orderInfoData"]["orderMemo"].InnerText.Replace(@"\", "");
        post = xnl["orderInfoData"]["receiverZonecode"].InnerText;
        recipient_addr1 = xnl["orderInfoData"]["receiverAddress"].InnerText.Replace(@"\", "");
        recipient_addr2 = xnl["orderInfoData"]["receiverAddressSub"].InnerText.Replace(@"\", "");

        XmlDocument m_xDocSub = new XmlDocument();
        m_xDocSub.LoadXml(xnl.OuterXml);
        XmlNodeList xmlListSub = m_xDocSub.SelectNodes("/order_data/orderGoodsData");
        foreach (XmlNode xnlSub in xmlListSub)
        {
            country = xnlSub["goodsModelNo"].InnerText;
            country_eng = "";
            commodity = xnlSub["goodsCd"].InnerText;
            commodity_eng = xnlSub["goodsNm"].InnerText;

            string optionInfo = xnlSub["optionInfo"].InnerText;
            optionInfo = optionInfo.Replace("[[", "").Replace("]]", "");

            string cut = "\"";

            List<string> optionInfos = new List<string>();

            string oinfo;
            int wi = 0;
            int locate;

            while (1 == 1)
            {
                if (optionInfo.Contains(cut))
                {
                    locate = optionInfo.IndexOf(cut);
                    optionInfo = optionInfo.Substring(locate + 1);
                    locate = optionInfo.IndexOf(cut);
                    oinfo = optionInfo.Substring(0, locate);
                    optionInfos.Add(oinfo);
                    optionInfo = optionInfo.Substring(locate + 1);
                }
                else
                {
                    break;
                }
                wi++;

            }

            unit = optionInfos[2];
            kg = optionInfos[0] + " : " + optionInfos[1];
            qty = xnlSub["goodsCnt"].InnerText;
            price = xnlSub["goodsPrice"].InnerText;
            o_price = xnlSub["optionPrice"].InnerText;
            sno = xnlSub["sno"].InnerText;


            GODO_LIST data = new GODO_LIST();
            data.CONTRACT_NO = contract_no;
            data.SNO = sno;
            data.COMPANY = company;
            data.CONTRACT_DATE = contract_date;
            data.ORDER_NM = order_nm;
            data.ORDER_EMAIL = order_email;
            data.ORDER_PHONE = order_phone;
            data.ORDER_NM2 = order_nm2;
            data.ORDER_PHONE2 = order_phone2;
            data.MEMO = memo;
            data.POST = post;
            data.RECIPIENT_ADDR1 = recipient_addr1;
            data.RECIPIENT_ADDR2 = recipient_addr2;
            data.COUNTRY = country;
            data.COUNTRY_ENG = country_eng;
            data.COMMODITY = commodity;
            data.COMMODITY_ENG = commodity_eng;
            data.UNIT = unit;
            data.KG = kg;
            data.QTY = qty;
            data.PRICE = Int32.Parse(price.Replace(".00", "")).ToString();
            data.O_PRICE = Int32.Parse(o_price.Replace(".00", "")).ToString();

            listdata.Add(data);

        }

        XmlNodeList xmlListGift = m_xDocSub.SelectNodes("/order_data/giftData");
        foreach (XmlNode xnlSub in xmlListGift)
        {
            GODO_GIFT_LIST giftData = new GODO_GIFT_LIST();
            giftData.CONTRACT_NO = contract_no;
            giftData.SNO = xnlSub["sno"].InnerText;
            giftData.GIFT_NO = xnlSub["giftNo"].InnerText;
            giftData.GIFT_CD = xnlSub["giftCd"].InnerText;
            giftData.GIFT_NM = xnlSub["giftNm"].InnerText;
            giftData.GIFT_CNT = xnlSub["giftCnt"].InnerText;

            giftdata.Add(giftData);
        }
    }
}

public class GODO_LIST
{
    string contract_no;     //주문번호
    string sno;             //주문상품 고유번호
    string company;         //주문자 이름
    string contract_date;   //주문일자(여기서는 업로드 일자)
    string order_nm;        //회원명
    string order_email;     //주문자 이메일
    string order_phone;     //주문자 핸드폰 번호
    string order_nm2;       //수취인 이름
    string order_phone2;    //수취인 핸드폰 번호
    string memo;            //주문시 남기는글
    string post;            //우편 번호
    string recipient_addr1; //수취인 주소
    string recipient_addr2; //수취인 나머지 주소
    string country;         //국가코드
    string country_eng;     //국가명(영)
    string commodity;       //품목코드
    string commodity_eng;   //품목명(영)
    string unit;            //중량 코드
    string kg;              //중량
    string qty;             //수량
    string price;           //판매가
    string o_price;         //옵션 금액

    public string CONTRACT_NO
    {
        get { return this.contract_no; }
        set { this.contract_no = value; }
    }
    public string SNO
    {
        get { return this.sno; }
        set { this.sno = value; }
    }
    public string COMPANY
    {
        get { return this.company; }
        set { this.company = value; }
    }
    public string CONTRACT_DATE
    {
        get { return this.contract_date; }
        set { this.contract_date = value; }
    }
    public string ORDER_NM
    {
        get { return this.order_nm; }
        set { this.order_nm = value; }
    }
    public string ORDER_EMAIL
    {
        get { return this.order_email; }
        set { this.order_email = value; }
    }
    public string ORDER_PHONE
    {
        get { return this.order_phone; }
        set { this.order_phone = value; }
    }
    public string ORDER_NM2
    {
        get { return this.order_nm2; }
        set { this.order_nm2 = value; }
    }
    public string ORDER_PHONE2
    {
        get { return this.order_phone2; }
        set { this.order_phone2 = value; }
    }
    public string MEMO
    {
        get { return this.memo; }
        set { this.memo = value; }
    }

    public string POST
    {
        get { return this.post; }
        set { this.post = value; }
    }
    public string RECIPIENT_ADDR1
    {
        get { return this.recipient_addr1; }
        set { this.recipient_addr1 = value; }
    }
    public string RECIPIENT_ADDR2
    {
        get { return this.recipient_addr2; }
        set { this.recipient_addr2 = value; }
    }
    public string COUNTRY
    {
        get { return this.country; }
        set { this.country = value; }
    }
    public string COUNTRY_ENG
    {
        get { return this.country_eng; }
        set { this.country_eng = value; }
    }
    public string COMMODITY
    {
        get { return this.commodity; }
        set { this.commodity = value; }
    }
    public string COMMODITY_ENG
    {
        get { return this.commodity_eng; }
        set { this.commodity_eng = value; }
    }
    public string UNIT
    {
        get { return this.unit; }
        set { this.unit = value; }
    }
    public string KG
    {
        get { return this.kg; }
        set { this.kg = value; }
    }
    public string QTY
    {
        get { return this.qty; }
        set { this.qty = value; }
    }
    public string PRICE
    {
        get { return this.price; }
        set { this.price = value; }
    }
    public string O_PRICE
    {
        get { return this.o_price; }
        set { this.o_price = value; }
    }

}

public class GODO_GIFT_LIST
{
    string contract_no;     //주문번호
    string sno;             //주문상품 고유번호
    string gift_no;         //주문자 이름
    string gift_cd;         //주문일자(여기서는 업로드 일자)
    string gift_nm;         //회원명
    string gift_cnt;        //주문자 이메일

    public string CONTRACT_NO
    {
        get { return this.contract_no; }
        set { this.contract_no = value; }
    }
    public string SNO
    {
        get { return this.sno; }
        set { this.sno = value; }
    }
    public string GIFT_NO
    {
        get { return this.gift_no; }
        set { this.gift_no = value; }
    }
    public string GIFT_CD
    {
        get { return this.gift_cd; }
        set { this.gift_cd = value; }
    }
    public string GIFT_NM
    {
        get { return this.gift_nm; }
        set { this.gift_nm = value; }
    }
    public string GIFT_CNT
    {
        get { return this.gift_cnt; }
        set { this.gift_cnt = value; }
    }

}
