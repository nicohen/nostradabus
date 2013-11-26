package com.nostradabus.application.geolocation;

import java.io.IOException;

public class PostResponse extends PropertiesStringParser {
    
    public PostResponse(String propertiesString) throws IOException {
        super(propertiesString);
    }
    
    public long getMinTimeInterval() {
        return Long.parseLong(getProperties().getProperty(ConfigurationConstants.MIN_TIME_INTERVAL));
    }
    
    public int getTrackerCount() {
        return Integer.parseInt(getProperties().getProperty("trackerCount"));
    }
    
}
