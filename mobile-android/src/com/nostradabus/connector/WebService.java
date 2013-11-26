package com.nostradabus.connector;

import com.nostradabus.connector.parser.ParseResult;

public interface WebService {

	ParseResult execute() throws Exception;

	void addParameter(String name, Object value);
	
	void addHeader(String name, String value);

	void addParameter(Object value);
}