namespace PeerTutoringSystem.Application.DTOs.Payment
{
    public class VietQrResponseDto
    {
        public string code { get; set; }
        public string desc { get; set; }
        public VietQrData data { get; set; }
    }

    public class VietQrData
    {
        public int acpId { get; set; }
        public string accountName { get; set; }
        public string qrCode { get; set; }
        public string qrDataURL { get; set; }
    }
}