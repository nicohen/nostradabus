package com.nostradabus.connector.parser;

import java.io.InputStream;

public interface WebServiceParser<E extends ParseResult> {
	public E parse(InputStream xml);
}
