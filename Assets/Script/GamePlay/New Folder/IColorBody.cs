
public interface IColorBody 
{
    public BusColor busColor { get; set; }

    public bool targetLocked { get; set; } // THÊM MỚI: Thuộc tính để xác định xem đối tượng có bị khóa mục tiêu hay không
    public int index { get; set; } // THÊM MỚI: Thuộc tính index để xác định vị trí của đối tượng trong chuỗi
    public  void OnHit() { }
}
