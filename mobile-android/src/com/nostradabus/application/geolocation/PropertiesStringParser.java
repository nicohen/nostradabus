package com.nostradabus.application.geolocation;

import java.io.ByteArrayInputStream;
import java.io.IOException;
import java.util.Properties;

import android.util.Log;

public abstract class PropertiesStringParser {

    private static final String TAG = "LiveTracker:PropertiesStringParser";
    
    private Properties properties;
    
    
    public PropertiesStringParser(String propertiesString) throws IOException {
        this.properties = parsePropertiesFromString(propertiesString);
    }
    
    
    protected Properties getProperties() {
        return properties;
    }

    protected Properties parsePropertiesFromString(String propertiesString) throws IOException {
        Properties properties = null;
        if (propertiesString != null) {
            if(propertiesString.charAt(0) == '<') {
                throw new IOException("Reveived HTML instead of properties!");
            }
            
            ByteArrayInputStream input = null;
            try {
                input = new ByteArrayInputStream(propertiesString.getBytes());
                properties = new Properties();
                properties.load(input);
            } catch (IOException e) {
                Log.e(TAG, e.getMessage());
            } finally {
                if (input != null) {
                    try {
                        input.close();
                    } catch (IOException e) {
                        Log.e(TAG, e.getMessage());
                    }
                }
            }
        }
        else {
            throw new IOException("Properties string must not be null!");
        }
        return properties;
    }
    
}
