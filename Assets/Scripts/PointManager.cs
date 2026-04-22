public class PointManager
{
    private float _totalPoints;
    private readonly float _maxTotalPoints;  // = จำนวน target ใน queue รอบนั้น

    public float TotalPoints => _totalPoints;
    public float FeverScore  => CalculatePoints();  // rename ให้ชัดขึ้น

    public PointManager(float maxTotalPoints)
    {
        _maxTotalPoints = maxTotalPoints;
    }

 
    /// เรียกหลัง GameManager ตรวจ match — isHit=true เพิ่ม 1 คะแนน
    public void AddPoints(float point)
    {
        _totalPoints += point;
    }


    /// fever = point / object.count ตาม flowchart
    public float CalculatePoints()
    {
        if (_maxTotalPoints <= 0) return 0f;
        return _totalPoints / _maxTotalPoints;
    }

    public void ResetPoints()
    {
        _totalPoints = 0;
    }
}