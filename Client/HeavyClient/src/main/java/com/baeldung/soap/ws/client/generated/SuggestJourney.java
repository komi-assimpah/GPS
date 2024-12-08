
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
 *         <element name="startLat" type="{http://www.w3.org/2001/XMLSchema}string" minOccurs="0"/>
 *         <element name="startLng" type="{http://www.w3.org/2001/XMLSchema}string" minOccurs="0"/>
 *         <element name="endLat" type="{http://www.w3.org/2001/XMLSchema}string" minOccurs="0"/>
 *         <element name="endLng" type="{http://www.w3.org/2001/XMLSchema}string" minOccurs="0"/>
 *         <element name="clientId" type="{http://www.w3.org/2001/XMLSchema}string" minOccurs="0"/>
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
    "startLat",
    "startLng",
    "endLat",
    "endLng",
    "clientId"
})
@XmlRootElement(name = "suggestJourney", namespace = "http://tempuri.org/")
public class SuggestJourney {

    @XmlElementRef(name = "startLat", namespace = "http://tempuri.org/", type = JAXBElement.class, required = false)
    protected JAXBElement<String> startLat;
    @XmlElementRef(name = "startLng", namespace = "http://tempuri.org/", type = JAXBElement.class, required = false)
    protected JAXBElement<String> startLng;
    @XmlElementRef(name = "endLat", namespace = "http://tempuri.org/", type = JAXBElement.class, required = false)
    protected JAXBElement<String> endLat;
    @XmlElementRef(name = "endLng", namespace = "http://tempuri.org/", type = JAXBElement.class, required = false)
    protected JAXBElement<String> endLng;
    @XmlElementRef(name = "clientId", namespace = "http://tempuri.org/", type = JAXBElement.class, required = false)
    protected JAXBElement<String> clientId;

    /**
     * Obtient la valeur de la propriété startLat.
     * 
     * @return
     *     possible object is
     *     {@link JAXBElement }{@code <}{@link String }{@code >}
     *     
     */
    public JAXBElement<String> getStartLat() {
        return startLat;
    }

    /**
     * Définit la valeur de la propriété startLat.
     * 
     * @param value
     *     allowed object is
     *     {@link JAXBElement }{@code <}{@link String }{@code >}
     *     
     */
    public void setStartLat(JAXBElement<String> value) {
        this.startLat = value;
    }

    /**
     * Obtient la valeur de la propriété startLng.
     * 
     * @return
     *     possible object is
     *     {@link JAXBElement }{@code <}{@link String }{@code >}
     *     
     */
    public JAXBElement<String> getStartLng() {
        return startLng;
    }

    /**
     * Définit la valeur de la propriété startLng.
     * 
     * @param value
     *     allowed object is
     *     {@link JAXBElement }{@code <}{@link String }{@code >}
     *     
     */
    public void setStartLng(JAXBElement<String> value) {
        this.startLng = value;
    }

    /**
     * Obtient la valeur de la propriété endLat.
     * 
     * @return
     *     possible object is
     *     {@link JAXBElement }{@code <}{@link String }{@code >}
     *     
     */
    public JAXBElement<String> getEndLat() {
        return endLat;
    }

    /**
     * Définit la valeur de la propriété endLat.
     * 
     * @param value
     *     allowed object is
     *     {@link JAXBElement }{@code <}{@link String }{@code >}
     *     
     */
    public void setEndLat(JAXBElement<String> value) {
        this.endLat = value;
    }

    /**
     * Obtient la valeur de la propriété endLng.
     * 
     * @return
     *     possible object is
     *     {@link JAXBElement }{@code <}{@link String }{@code >}
     *     
     */
    public JAXBElement<String> getEndLng() {
        return endLng;
    }

    /**
     * Définit la valeur de la propriété endLng.
     * 
     * @param value
     *     allowed object is
     *     {@link JAXBElement }{@code <}{@link String }{@code >}
     *     
     */
    public void setEndLng(JAXBElement<String> value) {
        this.endLng = value;
    }

    /**
     * Obtient la valeur de la propriété clientId.
     * 
     * @return
     *     possible object is
     *     {@link JAXBElement }{@code <}{@link String }{@code >}
     *     
     */
    public JAXBElement<String> getClientId() {
        return clientId;
    }

    /**
     * Définit la valeur de la propriété clientId.
     * 
     * @param value
     *     allowed object is
     *     {@link JAXBElement }{@code <}{@link String }{@code >}
     *     
     */
    public void setClientId(JAXBElement<String> value) {
        this.clientId = value;
    }

}
