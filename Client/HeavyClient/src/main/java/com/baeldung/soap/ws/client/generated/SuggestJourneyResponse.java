
package com.baeldung.soap.ws.client.generated;

import jakarta.xml.bind.JAXBElement;
import jakarta.xml.bind.annotation.XmlAccessType;
import jakarta.xml.bind.annotation.XmlAccessorType;
import jakarta.xml.bind.annotation.XmlElementRef;
import jakarta.xml.bind.annotation.XmlRootElement;
import jakarta.xml.bind.annotation.XmlType;


/**
 * <p>Classe Java pour anonymous complex type.
 * 
 * <p>Le fragment de schéma suivant indique le contenu attendu figurant dans cette classe.
 * 
 * <pre>{@code
 * <complexType>
 *   <complexContent>
 *     <restriction base="{http://www.w3.org/2001/XMLSchema}anyType">
 *       <sequence>
 *         <element name="suggestJourneyResult" type="{http://schemas.microsoft.com/2003/10/Serialization/Arrays}ArrayOfKeyValueOfstringItineraryo6oT2v6m" minOccurs="0"/>
 *       </sequence>
 *     </restriction>
 *   </complexContent>
 * </complexType>
 * }</pre>
 * 
 * 
 */
@XmlAccessorType(XmlAccessType.FIELD)
@XmlType(name = "", propOrder = {
    "suggestJourneyResult"
})
@XmlRootElement(name = "suggestJourneyResponse", namespace = "http://tempuri.org/")
public class SuggestJourneyResponse {

    @XmlElementRef(name = "suggestJourneyResult", namespace = "http://tempuri.org/", type = JAXBElement.class, required = false)
    protected JAXBElement<ArrayOfKeyValueOfstringItineraryo6OT2V6M> suggestJourneyResult;

    /**
     * Obtient la valeur de la propriété suggestJourneyResult.
     * 
     * @return
     *     possible object is
     *     {@link JAXBElement }{@code <}{@link ArrayOfKeyValueOfstringItineraryo6OT2V6M }{@code >}
     *     
     */
    public JAXBElement<ArrayOfKeyValueOfstringItineraryo6OT2V6M> getSuggestJourneyResult() {
        return suggestJourneyResult;
    }

    /**
     * Définit la valeur de la propriété suggestJourneyResult.
     * 
     * @param value
     *     allowed object is
     *     {@link JAXBElement }{@code <}{@link ArrayOfKeyValueOfstringItineraryo6OT2V6M }{@code >}
     *     
     */
    public void setSuggestJourneyResult(JAXBElement<ArrayOfKeyValueOfstringItineraryo6OT2V6M> value) {
        this.suggestJourneyResult = value;
    }

}
