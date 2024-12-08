
package com.baeldung.soap.ws.client.generated;

import java.util.ArrayList;
import java.util.List;
import jakarta.xml.bind.annotation.XmlAccessType;
import jakarta.xml.bind.annotation.XmlAccessorType;
import jakarta.xml.bind.annotation.XmlElement;
import jakarta.xml.bind.annotation.XmlType;


/**
 * <p>Classe Java pour ArrayOfKeyValueOfstringItineraryo6oT2v6m complex type.
 * 
 * <p>Le fragment de schéma suivant indique le contenu attendu figurant dans cette classe.
 * 
 * <pre>{@code
 * <complexType name="ArrayOfKeyValueOfstringItineraryo6oT2v6m">
 *   <complexContent>
 *     <restriction base="{http://www.w3.org/2001/XMLSchema}anyType">
 *       <sequence>
 *         <element name="KeyValueOfstringItineraryo6oT2v6m" maxOccurs="unbounded" minOccurs="0">
 *           <complexType>
 *             <complexContent>
 *               <restriction base="{http://www.w3.org/2001/XMLSchema}anyType">
 *                 <sequence>
 *                   <element name="Key" type="{http://www.w3.org/2001/XMLSchema}string"/>
 *                   <element name="Value" type="{http://schemas.datacontract.org/2004/07/ProxyCache.Models}Itinerary"/>
 *                 </sequence>
 *               </restriction>
 *             </complexContent>
 *           </complexType>
 *         </element>
 *       </sequence>
 *     </restriction>
 *   </complexContent>
 * </complexType>
 * }</pre>
 * 
 * 
 */
@XmlAccessorType(XmlAccessType.FIELD)
@XmlType(name = "ArrayOfKeyValueOfstringItineraryo6oT2v6m", namespace = "http://schemas.microsoft.com/2003/10/Serialization/Arrays", propOrder = {
    "keyValueOfstringItineraryo6OT2V6M"
})
public class ArrayOfKeyValueOfstringItineraryo6OT2V6M {

    @XmlElement(name = "KeyValueOfstringItineraryo6oT2v6m")
    protected List<ArrayOfKeyValueOfstringItineraryo6OT2V6M.KeyValueOfstringItineraryo6OT2V6M> keyValueOfstringItineraryo6OT2V6M;

    /**
     * Gets the value of the keyValueOfstringItineraryo6OT2V6M property.
     * 
     * <p>
     * This accessor method returns a reference to the live list,
     * not a snapshot. Therefore any modification you make to the
     * returned list will be present inside the Jakarta XML Binding object.
     * This is why there is not a {@code set} method for the keyValueOfstringItineraryo6OT2V6M property.
     * 
     * <p>
     * For example, to add a new item, do as follows:
     * <pre>
     *    getKeyValueOfstringItineraryo6OT2V6M().add(newItem);
     * </pre>
     * 
     * 
     * <p>
     * Objects of the following type(s) are allowed in the list
     * {@link ArrayOfKeyValueOfstringItineraryo6OT2V6M.KeyValueOfstringItineraryo6OT2V6M }
     * 
     * 
     * @return
     *     The value of the keyValueOfstringItineraryo6OT2V6M property.
     */
    public List<ArrayOfKeyValueOfstringItineraryo6OT2V6M.KeyValueOfstringItineraryo6OT2V6M> getKeyValueOfstringItineraryo6OT2V6M() {
        if (keyValueOfstringItineraryo6OT2V6M == null) {
            keyValueOfstringItineraryo6OT2V6M = new ArrayList<>();
        }
        return this.keyValueOfstringItineraryo6OT2V6M;
    }


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
     *         <element name="Key" type="{http://www.w3.org/2001/XMLSchema}string"/>
     *         <element name="Value" type="{http://schemas.datacontract.org/2004/07/ProxyCache.Models}Itinerary"/>
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
        "key",
        "value"
    })
    public static class KeyValueOfstringItineraryo6OT2V6M {

        @XmlElement(name = "Key", namespace = "http://schemas.microsoft.com/2003/10/Serialization/Arrays", required = true, nillable = true)
        protected String key;
        @XmlElement(name = "Value", namespace = "http://schemas.microsoft.com/2003/10/Serialization/Arrays", required = true, nillable = true)
        protected Itinerary value;

        /**
         * Obtient la valeur de la propriété key.
         * 
         * @return
         *     possible object is
         *     {@link String }
         *     
         */
        public String getKey() {
            return key;
        }

        /**
         * Définit la valeur de la propriété key.
         * 
         * @param value
         *     allowed object is
         *     {@link String }
         *     
         */
        public void setKey(String value) {
            this.key = value;
        }

        /**
         * Obtient la valeur de la propriété value.
         * 
         * @return
         *     possible object is
         *     {@link Itinerary }
         *     
         */
        public Itinerary getValue() {
            return value;
        }

        /**
         * Définit la valeur de la propriété value.
         * 
         * @param value
         *     allowed object is
         *     {@link Itinerary }
         *     
         */
        public void setValue(Itinerary value) {
            this.value = value;
        }

    }

}
