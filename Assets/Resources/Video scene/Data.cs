using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Data
{
    public decimal timestampMillisUTC;
    public int sysCalStatus, accCalStatus, gyroCalStatus, magnetoCalStatus;
    public float oriX, oriY, oriZ;
    public float accX, accY, accZ;
    public float latitude, longitude;

}

public class CsvLoader {
    private List<decimal> timestampsMillisList = new List<decimal>();
    private string dataFileNameNoGps;
    private string dataFileNameWithGps;
    private TextAsset impData;
    private List<Data> csvData = new List<Data>();

    public CsvType csvType;
    

    public CsvLoader (CsvType csvType, string noGps, string withGps){
        this.csvType = csvType;
        this.dataFileNameNoGps = noGps;
        this.dataFileNameWithGps = withGps;
    }

    public void loadCSV() {
        decimal firstTimestamp = 0;
        decimal currentTimestamp = 0;
        

        void addDataNoGps(){
            impData = Resources.Load<TextAsset>(dataFileNameNoGps);
            string[] data = impData.text.Split(new char[] { '\n' });     

            for (int i = 0; i < data.Length - 1; i++){
                string[] row = data[i].Split(new char[] { ',' });
                Data q = new Data();
                if (i == 0){
                    Decimal.TryParse(row[0], out firstTimestamp);
                    firstTimestamp = firstTimestamp / 1000;
                   
                } else {
                    Decimal.TryParse(row[0], out currentTimestamp);
                    currentTimestamp = currentTimestamp / 1000;
                    q.timestampMillisUTC = currentTimestamp - firstTimestamp;
                }

                int.TryParse(row[1], out q.sysCalStatus);     int.TryParse(row[3], out q.gyroCalStatus);
                int.TryParse(row[2], out q.accCalStatus);     int.TryParse(row[4], out q.magnetoCalStatus);

                float.TryParse(row[5], out q.oriX);           float.TryParse(row[8], out q.accX);
                float.TryParse(row[6], out q.oriY);           float.TryParse(row[9], out q.accY);
                float.TryParse(row[7], out q.oriZ);           float.TryParse(row[10], out q.accZ);                

                csvData.Add(q);
                
            }
        }

        void addDataWithGps(){
            impData = Resources.Load<TextAsset>(dataFileNameWithGps);
            string[] data = impData.text.Split(new char[] { '\n' });

            for (int i = 0; i < data.Length - 1; i++){
                Data q = new Data();
                string[] row = data[i].Split(new char[] { ',' });

                if (i == 0){ 
                    firstTimestamp = getMillis(row[0]);
                    q.timestampMillisUTC = 0;     
                } else {
                    currentTimestamp = getMillis(row[0]) - firstTimestamp;
                    q.timestampMillisUTC = currentTimestamp;
                }
                

                int.TryParse(row[1], out q.sysCalStatus);     int.TryParse(row[3], out q.gyroCalStatus);
                int.TryParse(row[2], out q.accCalStatus);     int.TryParse(row[4], out q.magnetoCalStatus);

                float.TryParse(row[5], out q.oriX);           float.TryParse(row[8], out q.accX);
                float.TryParse(row[6], out q.oriY);           float.TryParse(row[9], out q.accY);
                float.TryParse(row[7], out q.oriZ);           float.TryParse(row[10], out q.accZ);

                float.TryParse(row[11], out q.latitude);      float.TryParse(row[12], out q.longitude);

                csvData.Add(q);
                
        }

        decimal getMillis(string tsUTC){
            decimal timestamp;
            int year, month, day, hour, minute, second, millisecond;

            int.TryParse(tsUTC.Substring(0, 4), out year);
            int.TryParse(tsUTC.Substring(5, 2), out month);
            int.TryParse(tsUTC.Substring(8, 2), out day);

            int.TryParse(tsUTC.Substring(11, 2), out hour);
            int.TryParse(tsUTC.Substring(14, 2), out minute);
            int.TryParse(tsUTC.Substring(17, 2), out second);
            int.TryParse(tsUTC.Substring(20), out millisecond);

            timestamp = (decimal)(new DateTime(year, month, day, hour, minute, second, DateTimeKind.Utc))
                .Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc))
                    .TotalMilliseconds + (decimal)(millisecond * 0.001);
        
            return timestamp;
    }
        }

        void createArrayOfTimestamps(){
            for(int i = 0; i <= csvData.Count - 1; i++ ){
                timestampsMillisList.Add(csvData[i].timestampMillisUTC);
                //Debug.Log(csvData[i].timestampMillisUTC);
            }
        }
        
        if (csvType == CsvType.withGps){
            addDataWithGps();
            createArrayOfTimestamps();
        } else if (csvType == CsvType.noGps){
            addDataNoGps();
            createArrayOfTimestamps();
        }

    }
    
    public int searchForClosestTimestamp(decimal tsToSearchFor){
            decimal closestTimestamp = timestampsMillisList.Aggregate((x,y) => Math.Abs(x-tsToSearchFor) < Math.Abs(y-tsToSearchFor) ? x : y);
            int closestTimestampIndex = timestampsMillisList.IndexOf(closestTimestamp);
            return closestTimestampIndex;
    }

    public Quaternion getOrientationQuaternion(int index){
        Quaternion rotation = Quaternion.Euler(-(csvData[index].oriY), 0, csvData[index].oriX);
        return rotation;
    }

    public Quaternion getRoll(int index){
        Quaternion roll = Quaternion.Euler(0, 0, -(csvData[index].oriX));
        return roll;        
    }

    public Quaternion getPitch(int index, float rotationOffset){
        Quaternion pitch = Quaternion.Euler(csvData[index].oriY, 0 + rotationOffset, 0);
        return pitch;        
    }
    public double getXacc(int index){
        double currentXacc = csvData[index].accX;
        return currentXacc;
    }

    public double getYacc(int index){
        double currentYacc = csvData[index].accY;
        return currentYacc;
    }

    public double getZacc(int index){
        double currentZacc = csvData[index].accZ;
        return currentZacc;
    }

    public double getGravityFreeXacc(int index){
        double gravityFreeAcceleration = csvData[index].accX + Math.Sin(toRad(csvData[index].oriY + 0.88));
        return gravityFreeAcceleration;
    }

    public double getGravityFreeYacc(int index){
        double gravityFreeAcceleration = csvData[index].accY - Math.Sin(toRad(csvData[index].oriX));
        return gravityFreeAcceleration; 
    }
    //PARTAISIT Z ACC
    public double getGravityFreeZacc(int index){
        double gravityFreeAcceleration = csvData[index].accZ - Math.Sin(toRad(csvData[index].oriX));
        return gravityFreeAcceleration; 
    }    

    public double toRad(double degrees){
        double radians = (Math.PI / 180) * degrees;
        return radians;
    }
}
