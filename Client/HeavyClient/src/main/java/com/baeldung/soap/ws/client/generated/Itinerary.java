
package com.baeldung.soap.ws.client.generated;

import jakarta.xml.bind.JAXBElement;
import jakarta.xml.bind.annotation.XmlAccessType;
import jakarta.xml.bind.annotation.XmlAccessorType;
import jakarta.xml.bind.annotation.XmlElementRef;
import jakarta.xml.bind.annotation.XmlType;


/**
 * <p>Classe Java pour Itinerary complex type.
 * 
 * <p>Le fragment de schéma suivant indique le contenu attendu figurant dans cette classe.
 * 
 * <pre>{@code
 * <complexType name="Itinerary">
 *   <complexContent>
 *     <restriction base="{http://www.w3.org/2001/XMLSchema}anyType">
 *       <sequence>
 *         <element name="distance" type="{http://www.w3.org/2001/XMLSchema}double" minOccurs="0"/>
 *         <element name="duration" type="{http://www.w3.org/2001/XMLSchema}double" minOccurs="0"/>
 *         <element name="instructions" type="{http://schemas.datacontract.org/2004/07/ProxyCache.Models}ArrayOfInstruction" minOccurs="0"/>
 *       </sequence>
 *     </restriction>
 *   </complexContent>
 * </complexType>
 * }</pre>
 * 
 * 
 */
@XmlAccessorType(XmlAccessType.FIELD)
@XmlType(name = "Itinerary", propOrder = {
    "distance",
    "duration",
    "instructions"
})
public class Itinerary {

    protected Double distance;
    protected Double duration;
    @XmlElementRef(name = "instructions", namespace = "http://schemas.datacontract.org/2004/07/ProxyCache.Models", type = JAXBElement.class, required = false)
    protected JAXBElement<ArrayOfInstruction> instructions;

    /**
     * Obtient la valeur de la propriété distance.
     * 
     * @return
     *     possible object is
     *     {@link Double }
     *     
     */
    public Double getDistance() {
        return distance;
    }

    /**
     * Définit la valeur de la propriété distance.
     * 
     * @param value
     *     allowed object is
     *     {@link Double }
     *     
     */
    public void setDistance(Double value) {
        this.distance = value;
    }

    /**
     * Obtient la valeur de la propriété duration.
     * 
     * @return
     *     possible object is
     *     {@link Double }
     *     
     */
    public Double getDuration() {
        return duration;
    }

    /**
     * Définit la valeur de la propriété duration.
     * 
     * @param value
     *     allowed object is
     *     {@link Double }
     *     
     */
    public void setDuration(Double value) {
        this.duration = value;
    }

    /**
     * Obtient la valeur de la propriété instructions.
     * 
     * @return
     *     possible object is
     *     {@link JAXBElement }{@code <}{@link ArrayOfInstruction }{@code >}
     *     
     */
    public JAXBElement<ArrayOfInstruction> getInstructions() {
        return instructions;
    }

    /**
     * Définit la valeur de la propriété instructions.
     * 
     * @param value
     *     allowed object is
     *     {@link JAXBElement }{@code <}{@link ArrayOfInstruction }{@code >}
     *     
     */
    public void setInstructions(JAXBElement<ArrayOfInstruction> value) {
        this.instructions = value;
    }

}
