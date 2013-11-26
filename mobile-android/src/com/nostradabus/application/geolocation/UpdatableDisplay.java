package com.nostradabus.application.geolocation;

public interface UpdatableDisplay {

    public void updateLocationsSentCount(Integer count);

    public void updateLastLocationSentTime(Long time);
    
    public void updateMapPoint(Double latitude, Double longitude);
    
}
